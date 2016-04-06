//////////////////////////////////////////////////////////////////////
// Part of the LLBLGen Pro Driver for Sybase ASA, used in the generated code. 
// LLBLGen Pro is (c) 2002-2016 Solutions Design. All rights reserved.
// http://www.llblgen.com
//////////////////////////////////////////////////////////////////////
// This Driver's sourcecode is released under the following license:
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
using System.Linq;
using SD.Tools.BCLExtensions.DataRelated;
using SD.Tools.BCLExtensions.CollectionsRelated;
using SD.LLBLGen.Pro.DBDriverCore;
using System.Collections.Generic;
using System.Data.Common;

namespace SD.LLBLGen.Pro.DBDrivers.SybaseAsa
{
    /// <summary>
	/// SybaseAsa specific implementation of DBSchemaRetriever
	/// </summary>
	public class SybaseAsaSchemaRetriever : DBSchemaRetriever
	{
		/// <summary>
		/// CTor
		/// </summary>
		/// <param name="catalogRetriever">The catalog retriever.</param>
		public SybaseAsaSchemaRetriever(SybaseAsaCatalogRetriever catalogRetriever) : base(catalogRetriever)
		{
		}


		/// <summary>
		/// Retrieves the table- and field meta data for the tables which names are in the passed in elementNames and which are in the schema specified.
		/// </summary>
		/// <param name="schemaToFill">The schema to fill.</param>
		/// <param name="elementNames">The element names.</param>
		/// <remarks>Implementers should add DBTable instances with the DBTableField instances to the DBSchema instance specified.
		/// Default implementation is a no-op</remarks>
		protected override void RetrieveTableAndFieldMetaData(DBSchema schemaToFill, IEnumerable<DBElementName> elementNames)
		{
			#region Description of queries used
			//field query:
			//select tc.*, d.domain_name, d.[precision], r.remarks from systabcol tc
			//inner join sysdomain d on tc.domain_id = d.domain_id left join sysremark r on tc.object_id = r.object_id
			//where table_id = 
			//(
			//    select table_id from systab where table_name =?
			//    and creator = 
			//    (
			//        select user_id from sysuser where user_name='<schemaname>'
			//    )
			//)
			//
			// uc query
			//select i.index_name, tc.column_name
			//from sysidx i inner join sysidxcol ic
			//on i.table_id=ic.table_id
			//and i.index_id=ic.index_id
			//inner join systabcol tc
			//on ic.table_id = tc.table_id
			//and ic.column_id = tc.column_id
			//where i.table_id in
			//(
			//    select table_id from systab 
			//    where creator = 
			//    (
			//        select user_id from sysuser where user_name='<schemaname>'
			//    )
			//    and table_name=?
			//)
			// AND i.[unique] in (1, 2)
			// AND i.index_category=3
			#endregion

			DbConnection connection = this.DriverToUse.CreateConnection();
			DbCommand fieldCommand = this.DriverToUse.CreateCommand(connection, string.Format("select tc.*, d.domain_name, d.[precision], r.remarks from systabcol tc inner join sysdomain d on tc.domain_id = d.domain_id left join sysremark r on tc.object_id = r.object_id where table_id = (select table_id from systab where table_name = ? and creator = (select user_id from sysuser where user_name='{0}')) order by tc.column_id asc", schemaToFill.SchemaOwner));
			DbParameter tableNameParameter = this.DriverToUse.CreateParameter(fieldCommand, "@table_name", string.Empty);
			DbDataAdapter fieldAdapter = this.DriverToUse.CreateDataAdapter(fieldCommand);

			DbCommand pkCommand = this.DriverToUse.CreateStoredProcCallCommand(connection, "sp_pkeys");
			DbParameter pkRetrievalTableNameParameter = this.DriverToUse.CreateParameter(pkCommand, "@table_name", string.Empty);
 			this.DriverToUse.CreateParameter(pkCommand, "@table_owner", schemaToFill.SchemaOwner);
			DbDataAdapter pkRetrievalAdapter = this.DriverToUse.CreateDataAdapter(pkCommand);

            DbCommand ucCommand = this.DriverToUse.CreateCommand(connection, string.Format("select i.index_name, tc.column_name from sysidx i inner join sysidxcol ic on i.table_id=ic.table_id and i.index_id=ic.index_id inner join systabcol tc on ic.table_id = tc.table_id and ic.column_id = tc.column_id where i.table_id in ( select table_id from systab where creator = (select user_id from sysuser where user_name='{0}' ) and table_name=? ) AND i.[unique] in (1, 2) AND i.index_category=3", schemaToFill.SchemaOwner));
			DbParameter ucRetrievalTableNameParameter = this.DriverToUse.CreateParameter(ucCommand, "@table_name", string.Empty);
			DbDataAdapter ucRetrievalAdapter = this.DriverToUse.CreateDataAdapter(ucCommand);

			DataTable ucFieldsInTable = new DataTable();
			DataTable fieldsInTable = new DataTable();
			DataTable pkFieldsInTable = new DataTable();

			List<DBTable> tablesToRemove = new List<DBTable>();
			try
			{
				connection.Open();
				fieldCommand.Prepare();
				pkCommand.Prepare();
				ucCommand.Prepare();

				foreach(DBElementName tableName in elementNames)
				{
					DBTable currentTable = new DBTable(schemaToFill, tableName);
					schemaToFill.Tables.Add(currentTable);
					tableNameParameter.Value = currentTable.Name;

					// get the fields. 
					fieldsInTable.Clear();
					fieldAdapter.Fill(fieldsInTable);

					try
					{
						int ordinalPosition = 1;
						var fields = from row in fieldsInTable.AsEnumerable()
									 let typeDefinition = CreateTypeDefinition(row, "domain_name", "width")
									 let defaultValue = row.Value<string>("default") ?? string.Empty
									 let isIdentity = ((defaultValue=="autoincrement") || (row.Value<int>("max_identity") > 0))
									 select new DBTableField(row.Value<string>("column_name"), typeDefinition, row.Value<string>("remarks") ?? string.Empty)
									 {
										 OrdinalPosition = ordinalPosition++,
										 DefaultValue = defaultValue,
										 IsIdentity = isIdentity,
										 IsComputed = (row.Value<string>("column_type") == "C"),
										 IsNullable = (row.Value<string>("nulls") == "Y"),
										 IsTimeStamp = (typeDefinition.DBType==(int)AsaDbTypes.TimeStamp),
										 ParentTable = currentTable
									 };
						currentTable.Fields.AddRange(fields);

						// get Primary Key fields for this table
						pkRetrievalTableNameParameter.Value = currentTable.Name;
						pkFieldsInTable.Clear();
						pkRetrievalAdapter.Fill(pkFieldsInTable);
						foreach(DataRow row in pkFieldsInTable.AsEnumerable())
						{
							string columnName = row.Value<string>("column_name");
							DBTableField primaryKeyField = currentTable.FindFieldByName(columnName);
							if(primaryKeyField != null)
							{
								primaryKeyField.IsPrimaryKey = true;
								// PrimaryKeyConstraintName is not set, as ASA allows dropping and creating PKs without names. It's therefore not obtained (and not available either)
							}
						}

						// get UC fields for this table
						ucRetrievalTableNameParameter.Value = currentTable.Name;
						ucFieldsInTable.Clear();
						ucRetrievalAdapter.Fill(ucFieldsInTable);
						var ucFieldsPerUc = from row in ucFieldsInTable.AsEnumerable()
											group row by row.Value<string>("index_name") into g
											select g;

						foreach(IGrouping<string, DataRow> ucFields in ucFieldsPerUc)
						{
							DBUniqueConstraint currentUC = new DBUniqueConstraint(ucFields.Key) { AppliesToTable = currentTable };
							bool addUc = true;
							foreach(DataRow row in ucFields)
							{
								DBTableField currentField = currentTable.FindFieldByName(row.Value<string>("column_name"));
								if(currentField == null)
								{
									continue;
								}
								currentUC.Fields.Add(currentField);
							}
							addUc &= (currentUC.Fields.Count > 0);
							if(addUc)
							{
								currentTable.UniqueConstraints.Add(currentUC);
								currentUC.AppliesToTable = currentTable;
							}
						}
					}
					catch( ApplicationException ex )
					{
						// non fatal error, remove the table, proceed 
						schemaToFill.LogError( ex, "Table '" + currentTable.Name + "' removed from list due to an internal exception in Field population: " + ex.Message, "SybaseAsaSchemaRetriever::RetrieveTableAndFieldMetaData" );
						tablesToRemove.Add(currentTable);
					}
					catch( InvalidCastException ex )
					{
						// non fatal error, remove the table, proceed 
						schemaToFill.LogError(ex, "Table '" + currentTable.Name + "' removed from list due to cast exception in Field population.", "SybaseAsaSchemaRetriever::RetrieveTableAndFieldMetaData");
						tablesToRemove.Add(currentTable);
					}
				}
			}
			finally
			{
				connection.SafeClose(true);
			}
			foreach(DBTable toRemove in tablesToRemove)
			{
				schemaToFill.Tables.Remove(toRemove);
			}
		}
		

		/// <summary>
		/// Retrieves the view- and field meta data for the views which names are in the passed in elementNames and which are in the schema specified.
		/// </summary>
		/// <param name="schemaToFill">The schema to fill.</param>
		/// <param name="elementNames">The element names.</param>
		/// <remarks>Implementers should add DBView instances with the DBViewField instances to the DBSchema instance specified.
		/// Default implementation is a no-op</remarks>
		protected override void RetrieveViewAndFieldMetaData(DBSchema schemaToFill, IEnumerable<DBElementName> elementNames)
		{
			#region Description of queries used
			//field query:
			//select tc.*, d.domain_name, d.[precision], r.remarks from systabcol tc
			//inner join sysdomain d on tc.domain_id = d.domain_id left join sysremark r on tc.object_id = r.object_id
			//where table_id = 
			//(
			//    select table_id from systab where table_name =?
			//    and creator = 
			//    (
			//        select user_id from sysuser where user_name='<schemaname>'
			//    )
			//)
			#endregion
			DbConnection connection = this.DriverToUse.CreateConnection();

			DbCommand fieldCommand = this.DriverToUse.CreateCommand(connection, string.Format("select tc.*, d.domain_name, d.[precision], r.remarks from systabcol tc inner join sysdomain d on tc.domain_id = d.domain_id left join sysremark r on tc.object_id = r.object_id where table_id = (select table_id from systab where table_name = ? and creator = (select user_id from sysuser where user_name='{0}')) order by tc.column_id asc", schemaToFill.SchemaOwner));
			DbParameter tableNameParameter = this.DriverToUse.CreateParameter(fieldCommand, "@table_name", string.Empty);
			this.DriverToUse.CreateParameter(fieldCommand, "@table_owner", schemaToFill.SchemaOwner);
			DbDataAdapter fieldAdapter = this.DriverToUse.CreateDataAdapter(fieldCommand);
			DataTable fieldsInView = new DataTable();

			List<DBView> viewsToRemove = new List<DBView>();
			try
			{
				connection.Open();
				fieldCommand.Prepare();

				foreach(DBElementName viewName in elementNames)
				{
					DBView currentView = new DBView(schemaToFill, viewName);
					schemaToFill.Views.Add(currentView);
					tableNameParameter.Value = currentView.Name;

					// get the fields. 
					fieldsInView.Clear();
					fieldAdapter.Fill(fieldsInView);

					try
					{
						int ordinalPosition = 1;
						var fields = from row in fieldsInView.AsEnumerable()
									 let typeDefinition = CreateTypeDefinition(row, "domain_name", "width")
									 select new DBViewField(row.Value<string>("column_name"), typeDefinition, row.Value<string>("remarks") ?? string.Empty)
									 {
										 OrdinalPosition = ordinalPosition++,
										 IsNullable = (row.Value<string>("nulls") == "Y"),
										 ParentView = currentView
									 };
						currentView.Fields.AddRange(fields);
					}
					catch(ApplicationException ex)
					{
						// non fatal error, remove the view, proceed 
						schemaToFill.LogError(ex, "View '" + currentView.Name + "' removed from list due to an internal exception in Field population: " + ex.Message, "SybaseAsaSchemaRetriever::RetrieveViewAndFieldMetaData");
						viewsToRemove.Add(currentView);
					}
					catch(InvalidCastException ex)
					{
						// non fatal error, remove the view, proceed 
						schemaToFill.LogError(ex, "View '" + currentView.Name + "' removed from list due to cast exception in Field population.", "SybaseAsaSchemaRetriever::RetrieveViewAndFieldMetaData");
						viewsToRemove.Add(currentView);
					}
				}
			}
			finally
			{
				connection.SafeClose(true);
			}
			foreach(DBView toRemove in viewsToRemove)
			{
				schemaToFill.Views.Remove(toRemove);
			}
		}


		/// <summary>
		/// Retrieves the stored procedure- and parameter meta data for the stored procedures which names are in the passed in elementNames and which are
		/// in the schema specified.
		/// </summary>
		/// <param name="schemaToFill">The schema to fill.</param>
		/// <param name="elementNames">The element names.</param>
		/// <remarks>Implementers should add DBStoredProcedure instances with the DBStoredProcedureParameter instances to the DBSchema instance specified.
		/// Default implementation is a no-op</remarks>
		protected override void RetrieveStoredProcedureAndParameterMetaData(DBSchema schemaToFill, IEnumerable<DBElementName> elementNames)
		{
			#region Description of query used
			//select pp.*, d.domain_name, d.[precision]
			//from sysprocparm pp inner join sysprocedure p
			//on pp.proc_id = p.proc_id
			//inner join sysdomain d on pp.domain_id = d.domain_id
			//where pp.parm_type in (0, 1)
			//and p.proc_name = ?
			//and p.creator = (select user_id from sysuser where user_name='<schemaname>')
			//order by pp.parm_id ASC
			#endregion

			DbConnection connection = this.DriverToUse.CreateConnection();

			DbCommand command = this.DriverToUse.CreateCommand(connection, string.Format("select pp.*, d.domain_name, d.[precision] from sysprocparm pp inner join sysprocedure p on pp.proc_id = p.proc_id inner join sysdomain d on pp.domain_id = d.domain_id where pp.parm_type in (0, 1) and p.proc_name = ? and p.creator = (select user_id from sysuser where user_name='{0}') order by pp.parm_id ASC", schemaToFill.SchemaOwner));
			DbParameter procNameParameter = this.DriverToUse.CreateParameter(command, "@procedure_name", string.Empty);
			DbDataAdapter adapter = this.DriverToUse.CreateDataAdapter(command);
			DataTable parameterRows = new DataTable();
			List<DBStoredProcedure> storedProceduresToRemove = new List<DBStoredProcedure>();
			try
			{
				connection.Open();
				command.Prepare();

				List<string> storedProceduresAdded = new List<string>();
				foreach(DBElementName procName in elementNames)
				{
					DBStoredProcedure storedProcedure = new DBStoredProcedure(schemaToFill, procName);
					schemaToFill.StoredProcedures.Add(storedProcedure);
					storedProceduresAdded.Add(storedProcedure.Name);

					procNameParameter.Value = storedProcedure.Name;
					parameterRows.Clear();
					adapter.Fill(parameterRows);

					try
					{
						var parameters = from row in parameterRows.AsEnumerable()
										 where new[] { 0, 4}.Contains(row.Value<int>("parm_type"))
										 let typeDefinition = CreateTypeDefinition(row, "domain_name", "width")
										 select new DBStoredProcedureParameter(row.Value<string>("parm_name"), typeDefinition, row.Value<string>("remarks") ?? string.Empty)
										 {
											 OrdinalPosition = row.Value<int>("parm_id"),
											 Direction = DetermineParameterDirection(row),
											 ParentStoredProcedure = storedProcedure
										 };
						storedProcedure.Parameters.AddRange(p => p.OrdinalPosition, parameters);
					}
					catch(InvalidCastException ex)
					{
						// non fatal error, remove the stored procedure, proceed 
						schemaToFill.LogError(ex, "Stored procedure '" + storedProcedure.Name + "' removed from list due to cast exception in Parameter population.", "SybaseAsaSchemaRetriever::RetrieveStoredProcedureAndParameterMetaData");
						storedProceduresToRemove.Add(storedProcedure);
					}
					catch(ApplicationException ex)
					{
						// non fatal error, remove the table, proceed 
						schemaToFill.LogError(ex, "Stored procedure '" + storedProcedure.Name + "' removed from list due to an internal exception in Field population: " + ex.Message, "SybaseAsaSchemaRetriever::RetrieveStoredProcedureAndParameterMetaData");
						storedProceduresToRemove.Add(storedProcedure);
					}
				}
				// now add the procs which names are in the list of elementNames which aren't added yet, as these procs don't have parameters. 
				var procsWithoutParameters = elementNames.Except(storedProceduresAdded.Select(s => new DBElementName(s)));
				foreach(DBElementName procName in procsWithoutParameters)
				{
					schemaToFill.StoredProcedures.Add(new DBStoredProcedure(schemaToFill, procName));
				}
			}
			finally
			{
				connection.SafeClose(true);
			}
			foreach(DBStoredProcedure toRemove in storedProceduresToRemove)
			{
				schemaToFill.StoredProcedures.Remove(toRemove);
			}
		}


		/// <summary>
		/// Determines the parameter direction.
		/// </summary>
		/// <param name="row">The row.</param>
		/// <returns></returns>
		private static ParameterDirection DetermineParameterDirection(DataRow row)
		{
			ParameterDirection toReturn;

			bool input = (row.Value<string>("parm_mode_in") == "Y");
			bool output = (row.Value<string>("parm_mode_out") == "Y");
			bool returnValue = (row.Value<int>("parm_type") == 4);
			if(returnValue)
			{
				toReturn = ParameterDirection.ReturnValue;
			}
			else
			{
				if(input)
				{
					toReturn = output ? ParameterDirection.InputOutput : ParameterDirection.Input;
				}
				else
				{
					toReturn = ParameterDirection.Output;
				}
			}
			return toReturn;
		}


		/// <summary>
		/// creates a new type definition from the data in the row specified.
		/// </summary>
		/// <param name="row">The row.</param>
		/// <param name="columnNameTypeName">Name of the column name type.</param>
		/// <param name="columnNameLength">Length of the column name.</param>
		/// <returns>a filled DBTypeDefinition instance</returns>
		private DBTypeDefinition CreateTypeDefinition(DataRow row, string columnNameTypeName, string columnNameLength)
		{
			DBTypeDefinition toReturn = new DBTypeDefinition();
			string datatypeAsString = row.Value<string>(columnNameTypeName);
			int length = row.Value<int>(columnNameLength);
			int precision = row.Value<int?>("precision") ?? length;
			int dbType = SybaseAsaDBDriver.ConvertStringToDBType(datatypeAsString, length);
			switch((AsaDbTypes)dbType)
			{
				case AsaDbTypes.Image:
				case AsaDbTypes.LongBinary:
				case AsaDbTypes.LongNVarChar:
				case AsaDbTypes.LongVarChar:
					length = 0;	// unlimited.
					break;
			}
			int scale = row.Value<int?>("scale") ?? 0;
			switch((AsaDbTypes)dbType)
			{
				case AsaDbTypes.Image:
				case AsaDbTypes.LongBinary:
				case AsaDbTypes.LongNVarChar:
				case AsaDbTypes.LongVarChar:
					length = 0;	// unlimited.
					break;
			}
			toReturn.SetDBType(dbType, this.DriverToUse, length, precision, scale);
			return toReturn;
		}
	}
}
