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
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace SD.LLBLGen.Pro.DQE.SybaseAsa
{
	/// <summary>
	/// Implements IDbSpecificCreator for SybaseAsa. 
	/// </summary>
	[Serializable]
	public class SybaseAsaSpecificCreator : DbSpecificCreatorBase
	{
		#region Statics
		// this info is defined here and not in the base class because now a user can use more than one DQE at the same time with different providers.
		private static readonly DbProviderFactoryInfo _dbProviderFactoryInfo = new DbProviderFactoryInfo();
		#endregion

		/// <summary>
		/// CTor
		/// </summary>
		public SybaseAsaSpecificCreator()
		{
		}


		/// <summary>
		/// Sets the db provider factory parameter data. This will influence which DbProviderFactory is used and which enum types the field persistence info
		/// field type names are resolved to.
		/// </summary>
		/// <param name="dbProviderFactoryInvariantName">Name of the db provider factory invariant.</param>
		/// <param name="dbProviderSpecificEnumTypeName">Name of the db provider specific enum type.</param>
		/// <param name="dbProviderSpecificEnumTypePropertyName">Name of the db provider specific enum type property.</param>
		public static void SetDbProviderFactoryParameterData(string dbProviderFactoryInvariantName, string dbProviderSpecificEnumTypeName,
													  string dbProviderSpecificEnumTypePropertyName)
		{
			_dbProviderFactoryInfo.SetDbProviderFactoryParameterData(dbProviderFactoryInvariantName, dbProviderSpecificEnumTypeName, dbProviderSpecificEnumTypePropertyName);
		}

		/// <summary>
		/// Sets the db provider factory parameter data. This will influence which DbProviderFactory is used and which enum types the field persistence info
		/// field type names are resolved to.
		/// </summary>
		/// <param name="dbProviderFactoryInvariantNamesAndEnumTypeNames">The database provider factory invariant names and enum type names.</param>
		/// <param name="dbProviderSpecificEnumTypePropertyName">Name of the db provider specific enum type property.</param>
		public static void SetDbProviderFactoryParameterData(List<ValuePair<string, string>> dbProviderFactoryInvariantNamesAndEnumTypeNames, string dbProviderSpecificEnumTypePropertyName)
		{
			_dbProviderFactoryInfo.SetDbProviderFactoryParameterData(dbProviderFactoryInvariantNamesAndEnumTypeNames, dbProviderSpecificEnumTypePropertyName);
		}


		/// <summary>
		/// Determines the db type name for value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="realValueToUse">The real value to use. Normally it's the same as value, but in cases where value as a type isn't supported, the 
		/// value is converted to a value which is supported.</param>
		/// <returns>The name of the provider specific DbType enum name for the value specified</returns>
		public override string DetermineDbTypeNameForValue(object value, out object realValueToUse)
		{
			realValueToUse = value;
			string toReturn = "VarChar";
			if(value!=null)
			{
				switch(value.GetType().UnderlyingSystemType.FullName)
				{
					case "System.String":
						if(((string)value).Length < 4000)
						{
							toReturn = "NVarChar";
						}
						else
						{
							toReturn = ((string)value).Length < 8000 ? "VarChar" : "Text";
						}
						break;
					case "System.Byte":
						toReturn = "TinyInt";
						break;
					case "System.Int32":
						toReturn = "Integer";
						break;
					case "System.Int16":
						toReturn = "SmallInt";
						break;
					case "System.Int64":
						toReturn = "BigInt";
						break;
					case "System.DateTime":
						toReturn = "DateTime";
						break;
					case "System.Decimal":
						toReturn = "Decimal";
						break;
					case "System.Double":
						toReturn = "Double";
						break;
					case "System.Single":
						toReturn = "Real";
						break;
					case "System.Boolean":
						toReturn = "Bit";
						break;
					case "System.Byte[]":
						byte[] valueAsArray = (byte[])value;
						toReturn = valueAsArray.Length < 8000 ? "VarBinary" : "Image";
						break;
					default:
						toReturn = "VarChar";
						break;
				}
			}
			return toReturn;
		}


		/// <summary>
		/// Creates a valid Parameter for the pattern in a LIKE statement. This is a special case, because it shouldn't rely on the type of the
		/// field the LIKE statement is used with but should be the unicode varchar type.
		/// </summary>
		/// <param name="pattern">The pattern to be passed as the value for the parameter. Is used to determine length of the parameter.</param>
		/// <param name="targetFieldDbType">Type of the target field db, in provider specific enum string format (e.g. "Int" for SqlDbType.Int)</param>
		/// <returns>
		/// Valid parameter for usage with the target database.
		/// </returns>
		/// <remarks>This version ignores targetFieldDbType and calls CreateLikeParameter(fieldname, pattern). When you override this one, also
		/// override the other.</remarks>
		public override DbParameter CreateLikeParameter(string pattern, string targetFieldDbType)
		{
			string typeOfParameter = targetFieldDbType;
			switch(typeOfParameter)
			{
				case "Text":
					typeOfParameter = "VarChar";
					break;
				case "Char":
				case "NChar":
				case "NVarChar":
				case "VarChar":
					// keep type
					break;
				default:
					typeOfParameter = "NVarChar";
					break;
			}

			return this.CreateParameter(typeOfParameter, pattern.Length, ParameterDirection.Input, false, 0, 0, pattern);
		}


		/// <summary>
		/// Creates a valid object name (e.g. a name for a table or view) based on the fragments specified. The name is ready to  use and contains
		/// all alias wrappings required. 
		/// </summary>
		/// <param name="catalogName">Name of the catalog.</param>
		/// <param name="schemaName">Name of the schema.</param>
		/// <param name="elementName">Name of the element.</param>
		/// <returns>valid object name</returns>
		public override string CreateObjectName(string catalogName, string schemaName, string elementName)
		{
			StringBuilder name = new StringBuilder();
			string schemaNameToUse = new DynamicQueryEngine().GetNewSchemaName(this.GetNewPerCallSchemaName(schemaName));
			if(schemaNameToUse.Length > 0)
			{
				name.AppendFormat("{0}.", CreateValidAlias(schemaNameToUse));
			}
			name.AppendFormat("{0}", CreateValidAlias(elementName));
			return name.ToString();
		}


		/// <summary>
		/// Routine which creates a valid alias string for the raw alias passed in. For example, the alias will be surrounded by "[]" on sqlserver.
		/// Used by the RelationCollection to produce a valid alias for joins.
		/// </summary>
		/// <param name="rawAlias">the raw alias to make valid</param>
		/// <returns>valid alias string to use.</returns>
		public override string CreateValidAlias(string rawAlias)
		{
			if(string.IsNullOrEmpty(rawAlias))
			{
				return rawAlias;
			}
			if(rawAlias[0] == '[')
			{
				return rawAlias;
			} 
			return "[" + rawAlias + "]";
		}

		/// <inheritdoc />
		public override void AppendValidIdentifier(QueryFragments toAppendTo, string rawIdentifier)
		{
			if (string.IsNullOrEmpty(rawIdentifier))
			{
				return;
			}
			toAppendTo.AddFragment(rawIdentifier);
		}


		/// <summary>
		/// Creates a new dynamic query engine instance
		/// </summary>
		/// <returns></returns>
		protected override DynamicQueryEngineBase CreateDynamicQueryEngine()
		{
			return new DynamicQueryEngine();
		}


		/// <summary>
		/// Sets the ADO.NET provider specific Enum type of the parameter, using the string presentation specified.
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		/// <param name="parameterType">Type of the parameter as string.</param>
		protected override void SetParameterType(DbParameter parameter, string parameterType)
		{
			_dbProviderFactoryInfo.SetParameterType(parameter, parameterType);
		}


		/// <summary>
		/// Constructs a call to the aggregate function specified with the field name specified as parameter.
		/// </summary>
		/// <param name="function">The function.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns>
		/// ready to append string which represents the call to the aggregate function with the field as a parameter or the fieldname itself if
		/// the aggregate function isn't known.
		/// </returns>
		/// <remarks>Override this method and replace function with a function which is supported if your database doesn't support the function used.</remarks>
		protected override string ConstructCallToAggregateWithFieldAsParameter(AggregateFunction function, string fieldName)
		{
			AggregateFunction toUse = function;
			switch(toUse)
			{
				case AggregateFunction.CountBig:
					toUse = AggregateFunction.Count;
					break;
				case AggregateFunction.CountBigDistinct:
					toUse = AggregateFunction.CountDistinct;
					break;
				case AggregateFunction.CountBigRow:
					toUse = AggregateFunction.CountRow;
					break;
			}
			return base.ConstructCallToAggregateWithFieldAsParameter(toUse, fieldName);
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the DbProviderFactory instance to use.
		/// </summary>
		public override DbProviderFactory FactoryToUse
		{
			get { return _dbProviderFactoryInfo.FactoryToUse; }
		}
		#endregion
	}
}
