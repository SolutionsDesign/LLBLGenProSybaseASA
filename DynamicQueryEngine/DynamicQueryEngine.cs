//////////////////////////////////////////////////////////////////////
// Part of the Dynamic Query Engine (DQE) for Sybase ASA, used in the generated code. 
// LLBLGen Pro is (c) 2002-2016 Solutions Design. All rights reserved.
// http://www.llblgen.com
//////////////////////////////////////////////////////////////////////
// This DQE's sourcecode is released under the following license:
// --------------------------------------------------------------------------------------------
// 
// The MIT License(MIT)
//   
// Copyright (c)2002-2016 Solutions Design. All rights reserved.
// http://www.llblgen.com
//   
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//   
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//   
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//////////////////////////////////////////////////////////////////////
// Contributers to the code:
//		- Frans Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Collections;

namespace SD.LLBLGen.Pro.DQE.SybaseAsa
{
	/// <summary>
	/// DynamicQueryEngine for SybaseAsa.
	/// </summary>
	public class DynamicQueryEngine : DynamicQueryEngineBase
	{
		#region Static members
		private static readonly Dictionary<string, string> _schemaOverwrites;
		private static readonly Regex _procMatchingMatcher = new Regex(@"(?<schemaName>\[[\w\. \$@#]+\]|\w+).(?<procName>\[[\w\. \$@#]+\])", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		private static readonly FunctionMappingStore _functionMappings = new FunctionMappingStore();
		#endregion

		/// <summary>
		/// Creates a new <see cref="DynamicQueryEngine"/> instance.
		/// </summary>
		public DynamicQueryEngine() : base()
		{
		}

		/// <summary>
		/// Static CTor for initializing TraceSwitch and name overwrites
		/// </summary>
		static DynamicQueryEngine()
		{
			Switch = new TraceSwitch("SybaseAsaDQE", "Tracer for Sybase ASA Dynamic Query Engine");
			SybaseAsaSpecificCreator.SetDbProviderFactoryParameterData("Sap.Data.SQLAnywhere", "Sap.Data.SQLAnywhere.SADbType", "SADbType");

			_schemaOverwrites = new Dictionary<string, string>();
			NameValueCollection schemaOverwriteDefinitions = (NameValueCollection)ConfigurationManager.GetSection("sybaseAsaSchemaNameOverwrites");
			if (schemaOverwriteDefinitions != null)
			{
				for (int i = 0; i < schemaOverwriteDefinitions.Count; i++)
				{
					string key = schemaOverwriteDefinitions.GetKey(i);
					string value = schemaOverwriteDefinitions.Get(i);
					if (_schemaOverwrites.ContainsKey(key))
					{
						continue;
					}
					_schemaOverwrites.Add(key, value);
				}
			}
			CreateFunctionMappingStore();
		}

		#region Dynamic Insert Query construction methods.
		/// <summary>
		/// Creates a new Insert Query object which is ready to use.
		/// </summary>
		/// <param name="fields">Array of EntityFieldCore objects to use to build the insert query</param>
		/// <param name="fieldsPersistenceInfo">Array of IFieldPersistenceInfo objects to use to build the insert query</param>
		/// <param name="query">The query object to fill.</param>
		/// <param name="fieldToParameter">Hashtable which will contain after the call for each field the parameter which contains or will contain the field's value.</param>
		/// <remarks>Generic version.</remarks>
		/// <exception cref="System.ArgumentNullException">When fields is null or fieldsPersistenceInfo is null</exception>
		/// <exception cref="System.ArgumentException">When fields contains no EntityFieldCore instances or fieldsPersistenceInfo is empty.</exception>
		/// <exception cref="ORMQueryConstructionException">When there are no fields to insert in the fields list. This exception is to prevent
		/// INSERT INTO table () VALUES () style queries.</exception>
		protected override void CreateSingleTargetInsertDQ(IEntityFieldCore[] fields, IFieldPersistenceInfo[] fieldsPersistenceInfo,
															IActionQuery query, Dictionary<IEntityFieldCore, DbParameter> fieldToParameter)
		{
			TraceHelper.WriteLineIf(Switch.TraceInfo, "CreateSingleTargetInsertDQ", "Method Enter");
			QueryFragments fragments = new QueryFragments();
			fragments.AddFormatted("INSERT INTO {0}", this.Creator.CreateObjectName(fieldsPersistenceInfo[0]));
			var fieldNames = fragments.AddCommaDelimitedQueryFragments(true, 0);
			fragments.AddFragment("VALUES");
			var valueFragments = fragments.AddCommaDelimitedQueryFragments(true, 0);

			DbParameter newParameter;
			bool hasIdentity = false;
			for (int i = 0; i < fields.Length; i++)
			{
				IEntityFieldCore field = fields[i];
				IFieldPersistenceInfo persistenceInfo = fieldsPersistenceInfo[i];

				if (string.IsNullOrEmpty(persistenceInfo.IdentityValueSequenceName))
				{
					if (!CheckIfFieldNeedsInsertAction(field))
					{
						continue;
					}
					fieldNames.AddFragment(this.Creator.CreateFieldNameSimple(persistenceInfo, field.Name));
					AppendFieldToValueFragmentsForInsert(query, fieldToParameter, valueFragments, field, persistenceInfo);
				}
				else
				{
					newParameter = this.Creator.CreateParameter(field, persistenceInfo, ParameterDirection.InputOutput);
					query.AddParameterFieldRelation(field, newParameter, persistenceInfo.TypeConverterToUse, parameterValueCanBeNull: false);
					query.AddSequenceRetrievalQuery(CreateCommand("SELECT @@IDENTITY", query.Connection), false).AddSequenceParameter(newParameter);
					hasIdentity = true;
					fieldToParameter.Add(field, newParameter);
				}
			}
			if (fieldNames.Count <= 0)
			{
				if (hasIdentity)
				{
					// a table with just 1 identity field, use a special case query: INSERT INTO table values ()
					fragments.AddFragment("()");
				}
				else
				{
					throw new ORMQueryConstructionException("The insert query doesn't contain any fields.");
				}
			}
			query.SetCommandText(MakeParametersAnonymous(fragments.ToString(), query.Parameters));

			TraceHelper.WriteIf(Switch.TraceVerbose, query, "Generated Sql query");
			TraceHelper.WriteLineIf(Switch.TraceInfo, "CreateSingleTargetInsertDQ", "Method Exit");
		}
		#endregion

		#region Dynamic Delete Query construction methods.
		/// <summary>
		/// Creates a new Delete Query object which is ready to use.
		/// </summary>
		/// <param name="fieldsPersistenceInfo">Array of IFieldPersistenceInfo objects to use to build the delete query</param>
		/// <param name="query">The query object to fill.</param>
		/// <param name="deleteFilter">A complete IPredicate implementing object which contains the filter for the rows to delete</param>
		/// <remarks>Generic version</remarks>
		/// <exception cref="System.ArgumentNullException">When persistenceInfo is null</exception>
		protected override void CreateSingleTargetDeleteDQ(IFieldPersistenceInfo[] fieldsPersistenceInfo, IActionQuery query, IPredicate deleteFilter)
		{
			base.CreateSingleTargetDeleteDQ(fieldsPersistenceInfo, query, deleteFilter);
			// ASA doesn't support named parameters, anonymize them
			MakeParametersAnonymous(query.Command);
		}

		/// <summary>
		/// Creates a new Delete Query object which is ready to use.
		/// </summary>
		/// <param name="fieldsPersistenceInfo">Array of IFieldPersistenceInfo objects to use to build the delete query</param>
		/// <param name="query">The query object to fill.</param>
		/// <param name="deleteFilter">A complete IPredicate implementing object which contains the filter for the rows to delete</param>
		/// <param name="relationsToWalk">list of EntityRelation objects, which will be used to formulate a second FROM clause with INNER JOINs.</param>
		/// <remarks>Generic version</remarks>
		/// <exception cref="System.ArgumentNullException">When persistenceInfo is null or when deleteFilter is null or when relationsToWalk is null</exception>
		protected override void CreateSingleTargetDeleteDQ(IFieldPersistenceInfo[] fieldsPersistenceInfo, IActionQuery query, IPredicate deleteFilter,
															IRelationCollection relationsToWalk)
		{
			this.CreateSingleTargetDeleteDQUsingKeywordClause(fieldsPersistenceInfo, query, deleteFilter, relationsToWalk, "FROM");
			// ASA doesn't support named parameters, anonymize them
			MakeParametersAnonymous(query.Command);
		}
		#endregion

		#region Dynamic Update Query construction methods.
		/// <summary>
		/// Creates a new Update Query object which is ready to use. Only 'changed' EntityFieldCore fields are included in the update query.
		/// Primary Key fields are never updated.
		/// </summary>
		/// <param name="fields">EntityFieldCore array to use to build the update query.</param>
		/// <param name="fieldsPersistenceInfo">Array of IFieldPersistenceInfo objects to use to build the update query</param>
		/// <param name="query">The query object to fill.</param>
		/// <param name="updateFilter">A complete IPredicate implementing object which contains the filter for the rows to update</param>
		/// <exception cref="System.ArgumentNullException">When fields is null or fieldsPersistenceInfo is null</exception>
		/// <exception cref="System.ArgumentException">When fields contains no EntityFieldCore instances or fieldsPersistenceInfo is empty.</exception>
		protected override void CreateSingleTargetUpdateDQ(IEntityFieldCore[] fields, IFieldPersistenceInfo[] fieldsPersistenceInfo,
															IActionQuery query, IPredicate updateFilter)
		{
			base.CreateSingleTargetUpdateDQ(fields, fieldsPersistenceInfo, query, updateFilter);
			// ASA doesn't support named parameters, anonymize them
			MakeParametersAnonymous(query.Command);
		}


		/// <summary>
		/// Creates a new Update Query object which is ready to use. Only 'changed' EntityFieldCore are included in the update query.
		/// Primary Key fields are never updated.
		/// </summary>
		/// <param name="fields">Array of EntityFieldCore objects to use to build the insert query</param>
		/// <param name="fieldsPersistenceInfo">Array of IFieldPersistenceInfo objects to use to build the update query</param>
		/// <param name="query">The query object to fill.</param>
		/// <param name="updateFilter">A complete IPredicate implementing object which contains the filter for the rows to update</param>
		/// <param name="relationsToWalk">list of EntityRelation objects, which will be used to formulate a FROM clause with INNER JOINs.</param>
		/// <exception cref="System.ArgumentNullException">When fields is null or when updateFilter is null or
		/// when relationsToWalk is null or when fieldsPersistence is null</exception>
		/// <exception cref="System.ArgumentException">When fields contains no EntityFieldCore instances or fieldsPersistenceInfo is empty.</exception>
		protected override void CreateSingleTargetUpdateDQ(IEntityFieldCore[] fields, IFieldPersistenceInfo[] fieldsPersistenceInfo,
														   IActionQuery query, IPredicate updateFilter, IRelationCollection relationsToWalk)
		{
			this.CreateSingleTargetUpdateDQUsingFromClause(fields, fieldsPersistenceInfo, query, updateFilter, relationsToWalk);
			// ASA doesn't support named parameters, anonymize them
			MakeParametersAnonymous(query.Command);
		}
		#endregion

		#region Dynamic Select Query construction methods.
		/// <summary>
		/// Creates a new Select Query which is ready to use, based on the specified select list and the specified set of relations.
		/// If selectFilter is set to null, all rows are selected.
		/// </summary>
		/// <param name="selectList">list of IEntityFieldCore objects to select</param>
		/// <param name="fieldsPersistenceInfo">Array of IFieldPersistenceInfo objects to use to build the select query</param>
		/// <param name="query">The query to fill.</param>
		/// <param name="selectFilter">A complete IPredicate implementing object which contains the filter for the rows to select. When set to null, no
		/// filtering is done, and all rows are returned.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum number of items to return with this retrieval query.
		/// If the used Dynamic Query Engine supports it, 'TOP' is used to limit the amount of rows to return.
		/// When set to 0, no limitations are specified.</param>
		/// <param name="sortClauses">The order by specifications for the sorting of the resultset. When not specified, no sorting is applied.</param>
		/// <param name="relationsToWalk">list of EntityRelation objects, which will be used to formulate a FROM clause with INNER JOINs.</param>
		/// <param name="allowDuplicates">Flag which forces the inclusion of DISTINCT if set to true. If the resultset contains fields of type ntext, text or image, no duplicate filtering
		/// is done.</param>
		/// <param name="groupByClause">The list of fields to group by on. When not specified or an empty collection is specified, no group by clause
		/// is added to the query. A check is performed for each field in the selectList. If a field in the selectList is not present in the groupByClause
		/// collection, an exception is thrown.</param>
		/// <param name="relationsSpecified">flag to signal if relations are specified, this is a result of a check. This routine should
		/// simply assume the value of this flag is correct.</param>
		/// <param name="sortClausesSpecified">flag to signal if sortClauses are specified, this is a result of a check. This routine should
		/// simply assume the value of this flag is correct.</param>
		/// <exception cref="System.ArgumentNullException">When selectList is null or fieldsPersistenceInfo is null</exception>
		/// <exception cref="System.ArgumentException">When selectList contains no EntityFieldCore instances or fieldsPersistenceInfo is empty.</exception>
		protected override void CreateSelectDQ(IEntityFieldCore[] selectList, IFieldPersistenceInfo[] fieldsPersistenceInfo,
												IRetrievalQuery query, IPredicate selectFilter, long maxNumberOfItemsToReturn, ISortExpression sortClauses,
												IRelationCollection relationsToWalk, bool allowDuplicates, IGroupByCollection groupByClause,
												bool relationsSpecified, bool sortClausesSpecified)
		{
			TraceHelper.WriteLineIf(Switch.TraceInfo, "CreateSelectDQ", "Method Enter");

			QueryFragments fragments = new QueryFragments();
			fragments.AddFragment("SELECT");
			StringPlaceHolder distinctPlaceholder = fragments.AddPlaceHolder();
			StringPlaceHolder topPlaceholder = fragments.AddPlaceHolder();
			var projection = fragments.AddCommaDelimitedQueryFragments(false, 0);

			UniqueList<string> fieldNamesInSelectList;
			bool distinctViolatingTypesFound;
			bool pkFieldSeen;
			AppendResultsetFields(selectList, fieldsPersistenceInfo, relationsToWalk, projection, sortClausesSpecified, allowDuplicates, true, new UniqueList<string>(),
									query, out fieldNamesInSelectList, out distinctViolatingTypesFound, out pkFieldSeen);

			bool resultsCouldContainDuplicates = this.DetermineIfDuplicatesWillOccur(relationsToWalk);
			bool distinctEmitted = this.HandleDistinctEmit(sortClauses, allowDuplicates, sortClausesSpecified, query, distinctPlaceholder, distinctViolatingTypesFound,
														(pkFieldSeen && !resultsCouldContainDuplicates), fieldNamesInSelectList);

			bool groupByClauseSpecified = ((groupByClause != null) && (groupByClause.Count > 0));
			if (maxNumberOfItemsToReturn > 0)
			{
				// row limits are emitted always, unless duplicates are required but DISTINCT wasn't emitable. If not emitable, switch to client-side row limitation
				if (distinctEmitted || !resultsCouldContainDuplicates || groupByClauseSpecified || allowDuplicates)
				{
					topPlaceholder.SetFormatted("TOP {0}", maxNumberOfItemsToReturn);
				}
				else
				{
					query.RequiresClientSideLimitation = true;
					query.ManualRowsToTake = (int)maxNumberOfItemsToReturn;
				}
			}
			if (relationsSpecified)
			{
				fragments.AddFormatted("FROM {0}", relationsToWalk.ToQueryText());
				query.AddParameters(((RelationCollection)relationsToWalk).CustomFilterParameters);
			}
			else
			{
				var persistenceInfoToUse = fieldsPersistenceInfo.FirstOrDefault(p => p != null);
				if ((persistenceInfoToUse != null) && (persistenceInfoToUse.SourceObjectName.Length > 0))
				{
					fragments.AddFormatted(" FROM {0}", this.Creator.CreateObjectName(persistenceInfoToUse));
					string targetAlias = this.DetermineTargetAlias(selectList[0], relationsToWalk);
					if (targetAlias.Length > 0)
					{
						fragments.AddFormatted(" {0}", this.Creator.CreateValidAlias(targetAlias));
					}
				}
			}
			AppendWhereClause(selectFilter, fragments, query);
			AppendGroupByClause(groupByClause, fragments, query);
			AppendOrderByClause(sortClauses, fragments, query);
			query.SetCommandText(MakeParametersAnonymous(fragments.ToString(), query.Parameters));

			TraceHelper.WriteIf(Switch.TraceVerbose, query, "Generated Sql query");
			TraceHelper.WriteLineIf(Switch.TraceInfo, "CreateSelectDQ", "Method Exit");
		}


		/// <summary>
		/// Creates a new Select Query which is ready to use, based on the specified select list and the specified set of relations.
		/// If selectFilter is set to null, all rows are selected.
		/// </summary>
		/// <param name="selectList">list of IEntityFieldCore objects to select</param>
		/// <param name="fieldsPersistenceInfo">Array of IFieldPersistenceInfo objects to use to build the select query</param>
		/// <param name="connectionToUse">The connection to use for the query</param>
		/// <param name="selectFilter">A complete IPredicate implementing object which contains the filter for the rows to select. When set to null, no
		/// filtering is done, and all rows are returned.</param>
		/// <param name="rowsToSkip">The rows to skip. Default 0</param>
		/// <param name="rowsToTake">The rows to take. Default 0, which means all.</param>
		/// <param name="sortClauses">The order by specifications for the sorting of the resultset. When not specified, no sorting is applied.</param>
		/// <param name="relationsToWalk">list of EntityRelation objects, which will be used to formulate a FROM clause with INNER JOINs.</param>
		/// <param name="allowDuplicates">Flag which forces the inclusion of DISTINCT if set to true. If the resultset contains fields of type ntext, text or image, no duplicate filtering
		/// is done.</param>
		/// <param name="groupByClause">The list of fields to group by on. When not specified or an empty collection is specified, no group by clause
		/// is added to the query. A check is performed for each field in the selectList. If a field in the selectList is not present in the groupByClause
		/// collection, an exception is thrown.</param>
		/// <returns>
		/// IRetrievalQuery instance which is ready to be used.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">When selectList is null or fieldsPersistenceInfo is null or relationsToWalk is null</exception>
		/// <exception cref="System.ArgumentException">When selectList contains no EntityFieldCore instances or fieldsPersistenceInfo is empty.</exception>
		/// <remarks>
		/// Generic version
		/// </remarks>
		protected override IRetrievalQuery CreatePagingSelectDQ(IEntityFieldCore[] selectList, IFieldPersistenceInfo[] fieldsPersistenceInfo,
																DbConnection connectionToUse, IPredicate selectFilter, int rowsToSkip, int rowsToTake,
																ISortExpression sortClauses, IRelationCollection relationsToWalk, bool allowDuplicates,
																IGroupByCollection groupByClause)
		{
			TraceHelper.WriteLineIf(Switch.TraceInfo, "CreatePagingSelectDQ", "Method Enter");

			long max = 0;
			bool pagingRequired = true;
			if (rowsToSkip <= 0)
			{
				// no paging.
				max = rowsToTake;
				pagingRequired = false;
			}

			IRetrievalQuery normalQuery = this.CreateSelectDQ(selectList, fieldsPersistenceInfo, connectionToUse, selectFilter, max, sortClauses, relationsToWalk, allowDuplicates, groupByClause);
			if (!pagingRequired)
			{
				TraceHelper.WriteLineIf(Switch.TraceInfo, "CreatePagingSelectDQ: no paging.", "Method Exit");
				return normalQuery;
			}
			bool emitQueryToTrace = false;
			if (normalQuery.RequiresClientSideDistinctFiltering)
			{
				// manual paging required
				normalQuery.RequiresClientSidePaging = pagingRequired;
				normalQuery.ManualRowsToSkip = rowsToSkip;
				normalQuery.ManualRowsToTake = rowsToTake;
			}
			else
			{
				// normal paging. Embed paging logic. There is no TOP statement in the query as we've passed '0' for maxAmountOfItemsToReturn
				string upperLimitSnippet = string.Empty;
				if (rowsToTake > 0)
				{
					upperLimitSnippet = string.Format(" TOP {0}", rowsToTake);
				}
				if (normalQuery.Command.CommandText.ToLowerInvariant().StartsWith("select distinct"))
				{
					normalQuery.Command.CommandText = String.Format("SELECT DISTINCT{0} START AT {1} {2}",
																	upperLimitSnippet, rowsToSkip + 1, normalQuery.Command.CommandText.Substring(16));
				}
				else
				{
					normalQuery.Command.CommandText = String.Format("SELECT{0} START AT {1} {2}",
																	upperLimitSnippet, rowsToSkip + 1, normalQuery.Command.CommandText.Substring(7));
				}
				emitQueryToTrace = true;
			}

			TraceHelper.WriteIf(Switch.TraceVerbose && emitQueryToTrace, normalQuery, "Generated Sql query");
			TraceHelper.WriteLineIf(Switch.TraceInfo, "CreatePagingSelectDQ", "Method Exit");
			return normalQuery;
		}
		#endregion

		#region Name overwrite access methods
		/// <summary>
		/// Gets the new name for the schema, given the current name. If the current name is not found in the list of schema name overwrites, 
		/// the current name is returned. This routine works on the schema names specified in the config file.
		/// </summary>
		/// <param name="currentName">current Name</param>
		/// <returns>New name for the schema which name was passed in.</returns>
		/// <remarks>Thread safe, because the hashtable is never modified during execution.</remarks>
		public override string GetNewSchemaName(string currentName)
		{
			string toMatch = this.Creator.StripObjectNameChars(currentName);
			string toReturn = currentName;
			if (_schemaOverwrites.ContainsKey("*"))
			{
				toReturn = _schemaOverwrites["*"];
			}
			else
			{
				if ((_schemaOverwrites.Count > 0) && (_schemaOverwrites.ContainsKey(toMatch)))
				{
					toReturn = _schemaOverwrites[toMatch];
				}
			}
			return toReturn;
		}


		/// <summary>
		/// Gets the new name of the stored procedure passed in. Overwrites schema and catalog name with a new name if these names
		/// have been defined for overwriting. This routine works on the catalog and schema names specified in the config file.
		/// </summary>
		/// <param name="currentName">current Name</param>
		/// <remarks>Thread safe, because the hashtable is never modified during execution.</remarks>
		/// <returns>full stored procedure name with new catalog name/schema name.</returns>
		public override string GetNewStoredProcedureName(string currentName)
		{
			Regex procNamePartFinder = _procMatchingMatcher;

			MatchCollection matchesFound = procNamePartFinder.Matches(currentName);

			if (matchesFound.Count <= 0)
			{
				// just the proc name, or some weird format we don't support, return the proc name
				return currentName;
			}

			// there's just 1 match:
			string schemaName = matchesFound[0].Groups["schemaName"].Value;
			string procName = matchesFound[0].Groups["procName"].Value;

			return GetNewSchemaName(schemaName) + "." + procName;
		}


		/// <summary>
		/// Gets the new name of the stored procedure passed in. Overwrites schema and catalog name with a new name if these names
		/// have been defined for overwriting.  This routine works on the PerCallCatalogNameOverwrites and PerCallSchemaNameOverwrites names specified on this instance
		/// </summary>
		/// <param name="currentName">current Name</param>
		/// <remarks>Thread safe, because the hashtable is never modified during execution.</remarks>
		/// <returns>full stored procedure name with new catalog name/schema name.</returns>
		public override string GetNewPerCallStoredProcedureName(string currentName)
		{
			Regex procNamePartFinder = _procMatchingMatcher;
			MatchCollection matchesFound = procNamePartFinder.Matches(currentName);

			if (matchesFound.Count <= 0)
			{
				// just the proc name, or some weird format we don't support, return the proc name
				return currentName;
			}

			// there's just 1 match:
			string schemaName = matchesFound[0].Groups["schemaName"].Value;
			string procName = matchesFound[0].Groups["procName"].Value;

			return ((DbSpecificCreatorBase)this.Creator).GetNewPerCallSchemaName(schemaName) + "." + procName;
		}
		#endregion


		/// <summary>
		/// Creates the function mappings for this DQE.
		/// </summary>
		private static void CreateFunctionMappingStore()
		{
			// Parameter 0 is the element the method is called on. parameter 1 is the first argument etc. 
			// All indexes are converted to 0-based.
			////////////////////////////////////////////////
			// VB.NET compiler services specific methods
			////////////////////////////////////////////////
			// CompareString(3), which is emitted by the VB.NET compiler when a '=' operator is used between two string-typed operands.
			_functionMappings.Add(new FunctionMapping("Microsoft.VisualBasic.CompilerServices.Operators", "CompareString", 3, "CASE WHEN {0} < {1} THEN -1 WHEN {0} = {1} THEN 0 ELSE 1 END"));

			////////////////////////////////////////////////
			// Array related properties
			////////////////////////////////////////////////
			// Length
			_functionMappings.Add(new FunctionMapping(typeof(Array), "get_Length", 0, "LENGTH({0})"));

			////////////////////////////////////////////////
			// Boolean related functions
			////////////////////////////////////////////////
			// Negate(1) (!operand)
			_functionMappings.Add(new FunctionMapping(typeof(bool), "Negate", 1, "NOT ({0}=1)"));
			// ToString()
			_functionMappings.Add(new FunctionMapping(typeof(bool), "ToString", 0, "CASE WHEN ({0})=1 THEN 'True' ELSE 'False' END"));


			////////////////////////////////////////////////
			// Char related functions
			////////////////////////////////////////////////
			// ToUnicode(1) (new method, used to convert char to unicode. 
			_functionMappings.Add(new FunctionMapping(typeof(char), "ToUnicode", 1, "UNICODE({0})"));

			////////////////////////////////////////////////
			// Convert related functions
			////////////////////////////////////////////////
			// ToBoolean(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToBoolean", 1, "(CONVERT(BIT, {0})=1)"));
			// ToByte(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToByte", 1, "CONVERT(TINYINT, {0})"));
			// ToChar(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToChar", 1, "CONVERT(NCHAR, {0})"));
			// ToDateTime(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToDateTime", 1, "CONVERT(DATETIME, {0})"));
			// ToDecimal(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToDecimal", 1, "CONVERT(DECIMAL, {0})"));
			// ToDouble(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToDouble", 1, "CONVERT(FLOAT, {0})"));
			// ToInt16(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToInt16", 1, "CONVERT(SMALLINT, {0})"));
			// ToInt32(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToInt32", 1, "CONVERT(INT, {0})"));
			// ToInt64(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToInt64", 1, "CONVERT(BIGINT, {0})"));
			// ToSingle(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToSingle", 1, "CONVERT(REAL, {0})"));
			// ToString(1)
			_functionMappings.Add(new FunctionMapping(typeof(Convert), "ToString", 1, "CONVERT(NVARCHAR(4000), {0})"));

			////////////////////////////////////////////////
			// byte related functions
			////////////////////////////////////////////////
			_functionMappings.Add(new FunctionMapping(typeof(byte), "ToString", 0, "CONVERT(NVARCHAR(4), {0})"));
			_functionMappings.Add(new FunctionMapping(typeof(sbyte), "ToString", 0, "CONVERT(NVARCHAR(4), {0})"));

			////////////////////////////////////////////////
			// Int16 related functions
			////////////////////////////////////////////////
			_functionMappings.Add(new FunctionMapping(typeof(Int16), "ToString", 0, "CONVERT(NVARCHAR(5), {0})"));
			_functionMappings.Add(new FunctionMapping(typeof(UInt16), "ToString", 0, "CONVERT(NVARCHAR(5), {0})"));

			////////////////////////////////////////////////
			// Int32 related functions
			////////////////////////////////////////////////
			_functionMappings.Add(new FunctionMapping(typeof(Int32), "ToString", 0, "CONVERT(NVARCHAR(10), {0})"));
			_functionMappings.Add(new FunctionMapping(typeof(UInt32), "ToString", 0, "CONVERT(NVARCHAR(10), {0})"));

			////////////////////////////////////////////////
			// Int64 related functions
			////////////////////////////////////////////////
			_functionMappings.Add(new FunctionMapping(typeof(Int64), "ToString", 0, "CONVERT(NVARCHAR(32), {0})"));
			_functionMappings.Add(new FunctionMapping(typeof(UInt64), "ToString", 0, "CONVERT(NVARCHAR(32), {0})"));

			////////////////////////////////////////////////
			// DateTime related functions
			////////////////////////////////////////////////
			// AddDays(1)
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "AddDays", 1, "DATEADD(day, {1}, {0})"));
			// AddHours(1)
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "AddHours", 1, "DATEADD(hour, {1}, {0})"));
			// AddMilliseconds(1)
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "AddMilliseconds", 1, "DATEADD(millisecond, {1}, {0})"));
			// AddMinutes(1)
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "AddMinutes", 1, "DATEADD(minute, {1}, {0})"));
			// AddMonths(1)
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "AddMonths", 1, "DATEADD(month, {1}, {0})"));
			// AddSeconds(1)
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "AddSeconds", 1, "DATEADD(second, {1}, {0})"));
			// AddYears(1)
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "AddYears", 1, "DATEADD(year, {1}, {0})"));
			// Compare(2)
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "Compare", 2, "CASE WHEN {0} < {1} THEN -1 WHEN {0} = {1} THEN 0 ELSE 1 END"));
			////////////////////////////////////////////////
			// DateTime related properties
			////////////////////////////////////////////////
			// Date
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_Date", 0, "DATE({0})"));
			// Day
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_Day", 0, "DAY({0})"));
			// DayOfWeek
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_DayOfWeek", 0, "(DOW({0}) + (SELECT CONNECTION_PROPERTY('first_day_of_week')) + 6) % 7"));
			// DayOfYear
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_DayOfYear", 0, "DATEPART(dy, {0})"));
			// Hour
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_Hour", 0, "DATEPART(hh, {0})"));
			// Millisecond
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_Millisecond", 0, "DATEPART(ms, {0})"));
			// Minute
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_Minute", 0, "DATEPART(mi, {0})"));
			// Month
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_Month", 0, "MONTH({0})"));
			// Second
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_Second", 0, "DATEPART(ss, {0})"));
			// Year
			_functionMappings.Add(new FunctionMapping(typeof(DateTime), "get_Year", 0, "YEAR({0})"));

			////////////////////////////////////////////////
			// Decimal related functions
			////////////////////////////////////////////////
			// Add(2)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Add", 2, "({0} + {1})"));
			// Ceiling(1)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Ceiling", 1, "CEILING({0})"));
			// Compare(2)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Compare", 2, "CASE WHEN {0} < {1} THEN -1 WHEN {0} = {1} THEN 0 ELSE 1 END"));
			// Divide(2)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Divide", 2, "({0} / {1})"));
			// Floor(1)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Floor", 1, "FLOOR({0})"));
			// Multiply(2)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Multiply", 2, "({0} * {1})"));
			// Negate(1)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Negate", 1, "(-1 * {0})"));
			// Remainder(2)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Remainder", 2, "({0} - (CAST(({0} / {1}) AS INT) * {1}))"));
			// Round(1)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Round", 1, "ROUND({0}, 0)"));
			// Round(2)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Round", 2, "ROUND({0}, {1})"));
			// Substract(2)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Subtract", 2, "({0} - {1})"));
			// Truncate(1)
			_functionMappings.Add(new FunctionMapping(typeof(Decimal), "Truncate", 1, "TRUNCNUM({0}, 0)"));

			////////////////////////////////////////////////
			// Math related functions
			////////////////////////////////////////////////
			// Pow
			_functionMappings.Add(new FunctionMapping(typeof(Math), "Pow", 2, "POWER({0}, {1})"));

			////////////////////////////////////////////////
			// String related functions
			////////////////////////////////////////////////
			// Compare(2)
			_functionMappings.Add(new FunctionMapping(typeof(string), "Compare", 2, "CASE WHEN {0} < {1} THEN -1 WHEN {0} = {1} THEN 0 ELSE 1 END"));
			// Concat(2).
			_functionMappings.Add(new FunctionMapping(typeof(string), "Concat", 2, "({0} + {1})"));
			// IndexOf(1).
			_functionMappings.Add(new FunctionMapping(typeof(string), "IndexOf", 1, "(CHARINDEX({1}, {0}) - 1)"));
			// IndexOf(2)
			_functionMappings.Add(new FunctionMapping(typeof(string), "IndexOf", 2, "(CHARINDEX({1}, {0}, {2} + 1) - 1)"));
			// LastIndexOf(1)
			_functionMappings.Add(new FunctionMapping(typeof(string), "LastIndexOf", 1, "CASE WHEN COALESCE(CHARINDEX({1}, {0}), 0)=0 THEN -1 ELSE (LENGTH({0}) - CHARINDEX(REVERSE({1}), REVERSE({0})))-(LENGTH({1})-1) END"));
			// LastIndexOf(2)
			_functionMappings.Add(new FunctionMapping(typeof(string), "LastIndexOf", 2, "CASE WHEN LENGTH({0})<= {2} THEN -1 WHEN COALESCE(CHARINDEX({1}, LEFT({0}, {2})), 0)=0 THEN -1 ELSE ({2} - CHARINDEX(REVERSE({1}), REVERSE(LEFT({0}, {2}))))-(LENGTH({1})-1) END"));
			// PadLeft(1)
			_functionMappings.Add(new FunctionMapping(typeof(string), "PadLeft", 1, "CASE WHEN LENGTH({0})>={1} THEN {0} ELSE SPACE({1} - LENGTH({0})) + {0} END"));
			// PadLeft(2)
			_functionMappings.Add(new FunctionMapping(typeof(string), "PadLeft", 2, "CASE WHEN LENGTH({0})>={1} THEN {0} ELSE REPLICATE({2}, {1} - LENGTH({0})) + {0} END"));
			// PadRight(1)
			_functionMappings.Add(new FunctionMapping(typeof(string), "PadRight", 1, "CASE WHEN LENGTH({0})>={1} THEN {0} ELSE {0} + SPACE({1} - LENGTH({0})) END"));
			// PadRight(2)
			_functionMappings.Add(new FunctionMapping(typeof(string), "PadRight", 2, "CASE WHEN LENGTH({0})>={1} THEN {0} ELSE {0} + REPLICATE({2}, {1} - LENGTH({0})) END"));
			// Remove(1)
			_functionMappings.Add(new FunctionMapping(typeof(string), "Remove", 1, "LEFT({0}, {1})"));
			// Remove(2)
			_functionMappings.Add(new FunctionMapping(typeof(string), "Remove", 2, "STUFF({0}, {1}+1, {2}, '')"));
			// Replace(2)
			_functionMappings.Add(new FunctionMapping(typeof(string), "Replace", 2, "REPLACE({0}, {1}, {2})"));
			// Substring(1)
			_functionMappings.Add(new FunctionMapping(typeof(string), "Substring", 1, "SUBSTRING({0}, {1}+1)"));
			// Substring(2)
			_functionMappings.Add(new FunctionMapping(typeof(string), "Substring", 2, "SUBSTRING({0}, {1}+1, {2})"));
			// ToLower(0)
			_functionMappings.Add(new FunctionMapping(typeof(string), "ToLower", 0, "LOWER({0})"));
			// ToUpper(0)
			_functionMappings.Add(new FunctionMapping(typeof(string), "ToUpper", 0, "UPPER({0})"));
			// Trim(0)
			_functionMappings.Add(new FunctionMapping(typeof(string), "Trim", 0, "TRIM({0})"));
			////////////////////////////////////////////////
			// String related properties
			////////////////////////////////////////////////
			// Length
			_functionMappings.Add(new FunctionMapping(typeof(string), "get_Length", 0, "LENGTH({0})"));
			// Chars(1) / indexer
			_functionMappings.Add(new FunctionMapping(typeof(string), "get_Chars", 1, "SUBSTRING({0}, {1}+1, 1) "));

			////////////////////////////////////////////////
			// Object related functions
			////////////////////////////////////////////////
			// IIF(3). (IIF(op1, op2, op3) / op1 ? op2 : op3 statement)
			_functionMappings.Add(new FunctionMapping(typeof(object), "IIF", 3, "CASE WHEN {0}=1 THEN {1} ELSE {2} END"));
			// IIF(3). (IIF(op1, op2, op3) / op1 ? op2 : op3 statement). Used for boolean operands. The IIF will end up being wrapped with a boolean wrapper anyway, so it has to produce a boolean 
			_functionMappings.Add(new FunctionMapping(typeof(object), "IIF_Bool", 3, "(CASE WHEN {0}=1 THEN {1} ELSE {2} END)=1"));
			// LeftShift(2). (op1 << op2)
			_functionMappings.Add(new FunctionMapping(typeof(object), "LeftShift", 2, "({0} * POWER(2, {1}))"));
			// RightShift(2). (op1 >> op2)
			_functionMappings.Add(new FunctionMapping(typeof(object), "RightShift", 2, "({0} / POWER(2, {1}))"));
			// BooleanInProjectionWrapper(1)
			_functionMappings.Add(new FunctionMapping(typeof(object), "BooleanInProjectionWrapper", 1, "CASE WHEN {0} THEN 1 ELSE 0 END"));
		}


		/// <summary>
		/// Creates a new IDbSpecificCreator and initializes it
		/// </summary>
		/// <returns></returns>
		protected override IDbSpecificCreator CreateDbSpecificCreator()
		{
			return new SybaseAsaSpecificCreator();
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the function mappings for the particular DQE. These function mappings are static and therefore not changeable.
		/// </summary>
		public override FunctionMappingStore FunctionMappings
		{
			get { return _functionMappings; }
		}
		#endregion
	}
}