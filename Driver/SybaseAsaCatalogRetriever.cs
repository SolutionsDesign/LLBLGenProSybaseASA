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
using System.Collections.Generic;
using System.Linq;

using System.Data.Common;
using SD.Tools.BCLExtensions.DataRelated;
using SD.LLBLGen.Pro.DBDriverCore;

namespace SD.LLBLGen.Pro.DBDrivers.SybaseAsa
{
    /// <summary>
    /// SybaseAsa specific implementation of DBCatalogRetriever
    /// </summary>
	public class SybaseAsaCatalogRetriever : DBCatalogRetriever
    {
        /// <summary>
        /// CTor
        /// </summary>
        public SybaseAsaCatalogRetriever(SybaseAsaDBDriver driverToUse) : base(driverToUse)
        {
        }


		/// <summary>
		/// Produces the DBSchemaRetriever instance to use for retrieving meta-data of a schema.
		/// </summary>
		/// <returns>ready to use schema retriever</returns>
		protected override DBSchemaRetriever CreateSchemaRetriever()
		{
			return new SybaseAsaSchemaRetriever(this);
		}


		/// <summary>
		/// Retrieves all Foreign keys.
		/// </summary>
		/// <param name="catalogMetaData">The catalog meta data.</param>
		private void RetrieveForeignKeys(DBCatalog catalogMetaData)
		{
			#region Description of query used
			//select  fks.user_name AS FK_SCHEMA, fkt.table_name as FK_TABLE_NAME, fcol.column_name AS FK_COLUMN_NAME, 
			//        pks.user_name AS PK_SCHEMA, pkt.table_name as PK_TABLE_NAME, pcol.column_name AS PK_COLUMN_NAME, sc.constraint_name as FK_NAME
			//from    sysfkey fk inner join systab fkt on fk.foreign_table_id = fkt.table_id 
			//        inner join systab pkt on fk.primary_table_id = pkt.table_id 
			//        inner join sysuser fks on fkt.creator = fks.user_id 
			//        inner join sysuser pks on pkt.creator = pks.user_id 
			//        inner join sysidxcol fic on fic.table_id = fk.foreign_table_id and fic.index_id = fk.foreign_index_id 
			//        inner join sysidxcol pic on pic.table_id = fk.primary_table_id and pic.index_id = fk.primary_index_id 
			//                and fic.primary_column_id = pic.column_id
			//        inner join syscolumn fcol on fic.table_id=fcol.table_id and fic.column_id = fcol.column_id
			//        inner join syscolumn pcol on pic.table_id=pcol.table_id and pic.column_id = pcol.column_id
			//		  inner join sysidx si on si.table_id = fk.foreign_table_id and si.index_id = fk.foreign_index_id and si.index_category=2
			//		  inner join sysconstraint sc on sc.ref_object_id=si.object_id
			//where fks.user_name in (<schema list>) and pks.user_name in (<schema list>) 
			//order BY fks.user_name ASC, fkt.table_name ASC, fic.column_id ASC
			#endregion

			string inClause = String.Join(", ", catalogMetaData.Schemas.Select(s => string.Format("'{0}'", s.SchemaOwner)).ToArray());
			string query = string.Format("select  fks.user_name AS FK_SCHEMA, fkt.table_name as FK_TABLE_NAME, fcol.column_name AS FK_COLUMN_NAME, pks.user_name AS PK_SCHEMA, pkt.table_name as PK_TABLE_NAME, pcol.column_name AS PK_COLUMN_NAME, sc.constraint_name as FK_NAME from sysfkey fk inner join systab fkt on fk.foreign_table_id = fkt.table_id inner join systab pkt on fk.primary_table_id = pkt.table_id inner join sysuser fks on fkt.creator = fks.user_id inner join sysuser pks on pkt.creator = pks.user_id inner join sysidxcol fic on fic.table_id = fk.foreign_table_id and fic.index_id = fk.foreign_index_id inner join sysidxcol pic on pic.table_id = fk.primary_table_id and pic.index_id = fk.primary_index_id and fic.primary_column_id = pic.column_id inner join syscolumn fcol on fic.table_id=fcol.table_id and fic.column_id = fcol.column_id inner join syscolumn pcol on pic.table_id=pcol.table_id and pic.column_id = pcol.column_id inner join sysidx si on si.table_id = fk.foreign_table_id and si.index_id = fk.foreign_index_id and si.index_category=2 inner join sysconstraint sc on sc.ref_object_id=si.object_id where fks.user_name in ({0}) and pks.user_name in ({0}) order BY fks.user_name ASC, fkt.table_name ASC, fic.column_id ASC", inClause);
			DbDataAdapter adapter = this.DriverToUse.CreateDataAdapter(query);
			DataTable foreignKeys = new DataTable();
			adapter.Fill(foreignKeys);

			string currentPKTableName = string.Empty;
			string currentFKTableName = string.Empty;
			string currentFKName = string.Empty;

			// traverse per FK table name the fields which are stored in an FK constraint per different PK table name.
			DBForeignKeyConstraint newForeignKeyConstraint = new DBForeignKeyConstraint();
			DBTable tableForeignKey = null;
			DBTable tablePrimaryKey = null;
			bool fkValid = false;
			foreach(DataRow row in foreignKeys.AsEnumerable())
			{
				string previousPKTableName = currentPKTableName;
				currentPKTableName = row.Value<string>("PK_SCHEMA") + row.Value<string>("PK_TABLE_NAME");
				string previousFKTableName = currentFKTableName;
				currentFKTableName = row.Value<string>("FK_SCHEMA") + row.Value<string>("FK_TABLE_NAME");
				string previousFKName = currentFKName;
				currentFKName = row.Value<string>("FK_NAME");

				// if this is a new FK table, we've to start from scratch with a new FK constraint. If this isn't a new FK table, we've to check if this is a new 
				// PK table. if so, we've also to start from scratch with a new FK constraint. If this isn't a new PK table, we've to check whether the FK name 
				// changed. If so, we're dealing with a new FK constraint. Otherwise it's the same FK. 
				if((previousFKTableName != currentFKTableName) || (previousPKTableName != currentPKTableName) || (previousFKName != currentFKName))
				{
					// create a new FK 
					fkValid = true;
					newForeignKeyConstraint = new DBForeignKeyConstraint();
					newForeignKeyConstraint.ConstraintName = "FK_" + Guid.NewGuid().ToString("N");

					DBSchema schemaForeignKey = catalogMetaData.FindSchemaByName(row.Value<string>("FK_SCHEMA"));
					if(schemaForeignKey == null)
					{
						fkValid = false;
						continue;
					}
					tableForeignKey = schemaForeignKey.FindTableByName(row.Value<string>("FK_TABLE_NAME"));

					// Get Primary Key Table, first get the schema, has to be there
					DBSchema schemaPrimaryKey = catalogMetaData.FindSchemaByName(row.Value<string>("PK_SCHEMA"));
					if(schemaPrimaryKey == null)
					{
						fkValid = false;
						continue;
					}
					tablePrimaryKey = schemaPrimaryKey.FindTableByName(row.Value<string>("PK_TABLE_NAME"));

					if((tableForeignKey == null) || (tablePrimaryKey == null))
					{
						// not found. next
						fkValid = false;
						continue;
					}

					// Add to Foreign Key table. 
					tableForeignKey.ForeignKeyConstraints.Add(newForeignKeyConstraint);
				}

				// test again, if the FK is based on 2 or more fields, this test is required.
				if(!fkValid)
				{
					// not valid, skip
					if(tableForeignKey != null)
					{
						tableForeignKey.ForeignKeyConstraints.Remove(newForeignKeyConstraint);
					}
					continue;
				}

				newForeignKeyConstraint.AppliesToTable = tableForeignKey;
				DBTableField foreignKeyField = tableForeignKey.FindFieldByName(row.Value<string>("FK_COLUMN_NAME"));
				DBTableField primaryKeyField = tablePrimaryKey.FindFieldByName(row.Value<string>("PK_COLUMN_NAME"));
				if((foreignKeyField == null) || (primaryKeyField == null))
				{
					tableForeignKey.ForeignKeyConstraints.Remove(newForeignKeyConstraint);
					fkValid = false;
					continue;
				}
				newForeignKeyConstraint.PrimaryKeyFields.Add(primaryKeyField);
				newForeignKeyConstraint.ForeignKeyFields.Add(foreignKeyField);
			}
		}
		

		/// <summary>
		/// Produces the additional actions to perform by this catalog retriever
		/// </summary>
		/// <returns>list of additional actions to perform per schema</returns>
		private List<CatalogMetaDataRetrievalActionDescription> ProduceAdditionalActionsToPerform()
		{
			List<CatalogMetaDataRetrievalActionDescription> toReturn = new List<CatalogMetaDataRetrievalActionDescription>();
			toReturn.Add(new CatalogMetaDataRetrievalActionDescription("Retrieving all Foreign Key Constraints", (catalog) => RetrieveForeignKeys(catalog), false));
			return toReturn;
		}


		#region Class Property Declarations
		/// <summary>
		/// Gets the additional actions to perform per schema.
		/// </summary>
		protected override List<CatalogMetaDataRetrievalActionDescription> AdditionalActionsPerSchema
		{
			get { return ProduceAdditionalActionsToPerform(); }
		}
		#endregion
	}
}
