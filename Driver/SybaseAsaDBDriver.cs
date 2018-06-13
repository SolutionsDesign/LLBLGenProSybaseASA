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
using SD.Tools.BCLExtensions.CollectionsRelated;
using SD.Tools.BCLExtensions.DataRelated;
using SD.LLBLGen.Pro.DBDriverCore;
using System.Collections.Generic;
using System.Data.Common;

namespace SD.LLBLGen.Pro.DBDrivers.SybaseAsa
{
	/// <summary>
	/// General implementation of the SybaseAsa DBDriver.
	/// </summary>
	public class SybaseAsaDBDriver : DBDriverBase
	{
		#region Constants
		private const string driverType = "Sybase ASA DBDriver";
		private const string driverVersion = "5.0.20160127";
		private const string driverVendor = "Solutions Design bv";
		private const string driverCopyright = "(c)2002-2016 Solutions Design, all rights reserved.";
		private const string driverID = "3FABDE1A-21DF-4fcb-96FD-BBFA8F18B1EA";
		#endregion


		/// <summary>
		/// CTor
		/// </summary>
		public SybaseAsaDBDriver() : base((int)AsaDbTypes.AmountOfSqlDbTypes, driverType, driverVendor, driverVersion, driverCopyright, driverID, string.Empty)
		{
			InitDataStructures();
		}


		/// <summary>
		/// Gets the default name to use for schemas.
		/// </summary>
		/// <returns>the default name to use for schemas.</returns>
		public override string GetDefaultSchemaName()
		{
			return "dbo";
		}


		/// <summary>
		/// Produces the full name for command execution. The name returned is immediately usable as a name to target the element, e.g. a stored procedure.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>
		/// the name to use in a query to target the element specified.
		/// </returns>
		/// <remarks>By default it returns DBSchemaElement.FullName</remarks>
		protected override string ProduceFullNameForCommandExecution(DBSchemaElement element)
		{
			if(element.ContainingSchema == null)
			{
				return base.ProduceFullNameForCommandExecution(element);
			}
			return string.Format("[{0}].[{1}]", element.ContainingSchema.SchemaOwner, element.Name);
		}


		/// <summary>
		/// Fills the RDBMS functionality aspects.
		/// </summary>
		protected override void FillRdbmsFunctionalityAspects()
		{
			this.RdbmsFunctionalityAspects.Add(RdbmsFunctionalityAspect.AutoGenerateIdentityFields);
			this.RdbmsFunctionalityAspects.Add(RdbmsFunctionalityAspect.CentralUnitIsCatalog);
			this.RdbmsFunctionalityAspects.Add(RdbmsFunctionalityAspect.SupportsForeignKeyConstraints);
			this.RdbmsFunctionalityAspects.Add(RdbmsFunctionalityAspect.SupportsMultipleSchemasPerCentralUnit);
			this.RdbmsFunctionalityAspects.Add(RdbmsFunctionalityAspect.SupportsSchemaOnlyResultsetRetrieval);
			this.RdbmsFunctionalityAspects.Add(RdbmsFunctionalityAspect.SupportsNaturalCharacterSpecificTypes);
		}


		/// <summary>
		/// Fills the db type arrays for various conversions
		/// </summary>
		protected override void FillDbTypeConvertArrays()
		{
			this.DBTypesAsProviderType[(int)AsaDbTypes.BigInt] = "BigInt";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Binary] = "Binary";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Bit] = "Bit";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Char] = "Char";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Date] = "Date";
			this.DBTypesAsProviderType[(int)AsaDbTypes.DateTime] = "DateTime";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Decimal] = "Decimal";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Double] = "Double";
			this.DBTypesAsProviderType[(int)AsaDbTypes.FloatReal] = "Float";
			this.DBTypesAsProviderType[(int)AsaDbTypes.FloatDouble] = "Double";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Integer] = "Integer";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Image] = "Image";
			this.DBTypesAsProviderType[(int)AsaDbTypes.LongBinary] = "LongBinary";
			this.DBTypesAsProviderType[(int)AsaDbTypes.LongNVarChar] = "LongNVarchar";
			this.DBTypesAsProviderType[(int)AsaDbTypes.LongVarBit] = "LongVarbit";
			this.DBTypesAsProviderType[(int)AsaDbTypes.LongVarChar] = "LongVarchar";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Money] = "Money";
			this.DBTypesAsProviderType[(int)AsaDbTypes.NChar] = "NChar";
			this.DBTypesAsProviderType[(int)AsaDbTypes.NText] = "NText";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Numeric] = "Numeric";
			this.DBTypesAsProviderType[(int)AsaDbTypes.NVarChar] = "NVarChar";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Real] = "Real";
			this.DBTypesAsProviderType[(int)AsaDbTypes.SmallDateTime] = "SmallDateTime";
			this.DBTypesAsProviderType[(int)AsaDbTypes.SmallInt] = "SmallInt";
			this.DBTypesAsProviderType[(int)AsaDbTypes.SmallMoney] = "SmallMoney";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Text] = "Text";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Time] = "Time";
			this.DBTypesAsProviderType[(int)AsaDbTypes.TimeStamp] = "TimeStamp";
			this.DBTypesAsProviderType[(int)AsaDbTypes.TinyInt] = "TinyInt";
			this.DBTypesAsProviderType[(int)AsaDbTypes.UniqueIdentifier] = "UniqueIdentifier";
			this.DBTypesAsProviderType[(int)AsaDbTypes.UnsignedBigInt] = "UnsignedBigInt";
			this.DBTypesAsProviderType[(int)AsaDbTypes.UnsignedInt] = "UnsignedInt";
			this.DBTypesAsProviderType[(int)AsaDbTypes.UnsignedSmallInt] = "UnsignedSmallInt";
			this.DBTypesAsProviderType[(int)AsaDbTypes.VarBinary] = "VarBinary";
			this.DBTypesAsProviderType[(int)AsaDbTypes.VarBit] = "VarBit";
			this.DBTypesAsProviderType[(int)AsaDbTypes.VarChar] = "VarChar";
			this.DBTypesAsProviderType[(int)AsaDbTypes.Xml] = "Xml";

			this.DBTypesAsNETType[(int)AsaDbTypes.BigInt] = typeof(Int64);
			this.DBTypesAsNETType[(int)AsaDbTypes.Binary] = typeof(Byte[]);
			this.DBTypesAsNETType[(int)AsaDbTypes.Bit] = typeof(Boolean);
			this.DBTypesAsNETType[(int)AsaDbTypes.Char] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.Date] = typeof(DateTime);
			this.DBTypesAsNETType[(int)AsaDbTypes.DateTime] = typeof(DateTime);
			this.DBTypesAsNETType[(int)AsaDbTypes.Decimal] = typeof(Decimal);
			this.DBTypesAsNETType[(int)AsaDbTypes.Double] = typeof(Double);
			this.DBTypesAsNETType[(int)AsaDbTypes.FloatReal] = typeof(Single);
			this.DBTypesAsNETType[(int)AsaDbTypes.FloatDouble] = typeof(Double);
			this.DBTypesAsNETType[(int)AsaDbTypes.Integer] = typeof(Int32);
			this.DBTypesAsNETType[(int)AsaDbTypes.Image] = typeof(Byte[]);
			this.DBTypesAsNETType[(int)AsaDbTypes.LongBinary] = typeof(Byte[]);
			this.DBTypesAsNETType[(int)AsaDbTypes.LongNVarChar] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.LongVarBit] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.LongVarChar] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.Money] = typeof(Decimal);
			this.DBTypesAsNETType[(int)AsaDbTypes.NChar] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.NText] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.Numeric] = typeof(Decimal);
			this.DBTypesAsNETType[(int)AsaDbTypes.NVarChar] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.Real] = typeof(Single);
			this.DBTypesAsNETType[(int)AsaDbTypes.SmallDateTime] = typeof(DateTime);
			this.DBTypesAsNETType[(int)AsaDbTypes.SmallInt] = typeof(Int16);
			this.DBTypesAsNETType[(int)AsaDbTypes.SmallMoney] = typeof(Decimal);
			this.DBTypesAsNETType[(int)AsaDbTypes.Text] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.Time] = typeof(TimeSpan);
			this.DBTypesAsNETType[(int)AsaDbTypes.TimeStamp] = typeof(DateTime);
			this.DBTypesAsNETType[(int)AsaDbTypes.TinyInt] = typeof(Byte);
			this.DBTypesAsNETType[(int)AsaDbTypes.UniqueIdentifier] = typeof(Guid);
			this.DBTypesAsNETType[(int)AsaDbTypes.UnsignedBigInt] = typeof(UInt64);
			this.DBTypesAsNETType[(int)AsaDbTypes.UnsignedInt] = typeof(UInt32);
			this.DBTypesAsNETType[(int)AsaDbTypes.UnsignedSmallInt] = typeof(UInt16);
			this.DBTypesAsNETType[(int)AsaDbTypes.VarBinary] = typeof(Byte[]);
			this.DBTypesAsNETType[(int)AsaDbTypes.VarBit] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.VarChar] = typeof(String);
			this.DBTypesAsNETType[(int)AsaDbTypes.Xml] = typeof(String);

			this.DBTypesAsString[(int)AsaDbTypes.BigInt] = "BIGINT";
			this.DBTypesAsString[(int)AsaDbTypes.Binary] = "BINARY";
			this.DBTypesAsString[(int)AsaDbTypes.Bit] = "BIT";
			this.DBTypesAsString[(int)AsaDbTypes.Char] = "CHAR";
			this.DBTypesAsString[(int)AsaDbTypes.Date] = "DATE";
			this.DBTypesAsString[(int)AsaDbTypes.DateTime] = "DATETIME";
			this.DBTypesAsString[(int)AsaDbTypes.Decimal] = "DECIMAL";
			this.DBTypesAsString[(int)AsaDbTypes.Double] = "DOUBLE";
			this.DBTypesAsString[(int)AsaDbTypes.FloatReal] = "FLOAT";
			this.DBTypesAsString[(int)AsaDbTypes.FloatDouble] = "FLOAT";
			this.DBTypesAsString[(int)AsaDbTypes.Image] = "IMAGE";
			this.DBTypesAsString[(int)AsaDbTypes.Integer] = "INTEGER";
			this.DBTypesAsString[(int)AsaDbTypes.LongBinary] = "LONG BINARY";
			this.DBTypesAsString[(int)AsaDbTypes.LongNVarChar] = "LONG NVARCHAR";
			this.DBTypesAsString[(int)AsaDbTypes.LongVarBit] = "LONG VARBIT";
			this.DBTypesAsString[(int)AsaDbTypes.LongVarChar] = "LONG VARCHAR";
			this.DBTypesAsString[(int)AsaDbTypes.Money] = "MONEY";
			this.DBTypesAsString[(int)AsaDbTypes.NChar] = "NCHAR";
			this.DBTypesAsString[(int)AsaDbTypes.NText] = "NTEXT";
			this.DBTypesAsString[(int)AsaDbTypes.Numeric] = "NUMERIC";
			this.DBTypesAsString[(int)AsaDbTypes.NVarChar] = "NVARCHAR";
			this.DBTypesAsString[(int)AsaDbTypes.Real] = "REAL";
			this.DBTypesAsString[(int)AsaDbTypes.SmallDateTime] = "SMALLDATETIME";
			this.DBTypesAsString[(int)AsaDbTypes.SmallInt] = "SMALLINT";
			this.DBTypesAsString[(int)AsaDbTypes.SmallMoney] = "SMALLMONEY";
			this.DBTypesAsString[(int)AsaDbTypes.Text] = "TEXT";
			this.DBTypesAsString[(int)AsaDbTypes.Time] = "TIME";
			this.DBTypesAsString[(int)AsaDbTypes.TimeStamp] = "TIMESTAMP";
			this.DBTypesAsString[(int)AsaDbTypes.TinyInt] = "TINYINT";
			this.DBTypesAsString[(int)AsaDbTypes.UniqueIdentifier] = "UNIQUEIDENTIFIER";
			this.DBTypesAsString[(int)AsaDbTypes.UnsignedBigInt] = "UNSIGNED BIGINT";
			this.DBTypesAsString[(int)AsaDbTypes.UnsignedInt] = "UNSIGNED INT";
			this.DBTypesAsString[(int)AsaDbTypes.UnsignedSmallInt] = "UNSIGNED SMALLINT";
			this.DBTypesAsString[(int)AsaDbTypes.VarBinary] = "VARBINARY";
			this.DBTypesAsString[(int)AsaDbTypes.VarBit] = "VARBIT";
			this.DBTypesAsString[(int)AsaDbTypes.VarChar] = "VARCHAR";
			this.DBTypesAsString[(int)AsaDbTypes.Xml] = "XML";
		}


		/// <summary>
		/// Gets the string value of the db type passed in, from the Enum specification used in this driver for type specification
		/// </summary>
		/// <param name="dbType">The db type value.</param>
		/// <returns>string representation of the dbType specified when seen as a value in the type enum used by this driver to specify types.</returns>
		public override string GetDbTypeAsEnumStringValue(int dbType)
		{
			if(!Enum.IsDefined(typeof(AsaDbTypes), dbType))
			{
				return "INVALID";
			}
			return ((AsaDbTypes)dbType).ToString();
		}
		

		/// <summary>
		/// Fills the NET to DB type conversions list.
		/// </summary>
		protected override void FillNETToDBTypeConversionsList()
		{
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(byte[]), l => (l == 0) || (l >= 32768), null, null, (int)AsaDbTypes.Image, 2147483647, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(byte[]), l => (l == 0) || (l >= 32768), null, null, (int)AsaDbTypes.LongBinary, 2147483647, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(byte[]), l => (l > 0) && (l < 32768), null, null, (int)AsaDbTypes.Binary, -1, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(byte[]), l => (l > 0) && (l < 32768), null, null, (int)AsaDbTypes.VarBinary, -1, 0, 0));

			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l == 0) || (l >= 8192), null, null, (int)AsaDbTypes.Text, 2147483647, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l == 0) || (l >= 8192), null, null, (int)AsaDbTypes.NText, 2147483647, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l == 0) || (l >= 8192), null, null, (int)AsaDbTypes.LongVarChar, 2147483647, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l == 0) || (l >= 8192), null, null, (int)AsaDbTypes.LongNVarChar, 2147483647, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l == 0) || (l >= 8192), null, null, (int)AsaDbTypes.Xml, 2147483647, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l > 0) && (l < 8192), null, null, (int)AsaDbTypes.Char, -1, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l > 0) && (l < 8192), null, null, (int)AsaDbTypes.NChar, -1, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l > 0) && (l < 8192), null, null, (int)AsaDbTypes.NVarChar, -1, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(string), l => (l > 0) && (l < 8192), null, null, (int)AsaDbTypes.VarChar, -1, 0, 0));

			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(DateTime), (int)AsaDbTypes.Date, 0, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(DateTime), (int)AsaDbTypes.DateTime, 0, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(DateTime), (int)AsaDbTypes.SmallDateTime, 0, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(DateTime), (int)AsaDbTypes.TimeStamp, 0, 0, 0));

			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(decimal), null, p=> p>0 && p<127, s => s == 0, (int)AsaDbTypes.Numeric, 0, -1, -1));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(decimal), null, p => p > 0 && p <= 10, s => s > 0 && s <= 4, (int)AsaDbTypes.SmallMoney, 0, -1, -1));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(decimal), null, p => p > 0 && p <= 19, s => s > 0 && s <= 4, (int)AsaDbTypes.Money, 0, -1, -1));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(decimal), null, p=> p > 0 && p < 127, null, (int)AsaDbTypes.Decimal, 0, -1, -1));

			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(float), (int)AsaDbTypes.Real, 0, -1, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(double), (int)AsaDbTypes.Double, 0, -1, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(long), (int)AsaDbTypes.BigInt, 0, 19, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(bool), (int)AsaDbTypes.Bit, 0, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(int), (int)AsaDbTypes.Integer, 0, 10, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(short), (int)AsaDbTypes.SmallInt, 0, 5, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(byte), (int)AsaDbTypes.TinyInt, 0, 3, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(TimeSpan), (int)AsaDbTypes.Time, 0, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(Guid), (int)AsaDbTypes.UniqueIdentifier, 0, 0, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(UInt64), (int)AsaDbTypes.UnsignedBigInt, 0, 19, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(UInt32), (int)AsaDbTypes.UnsignedInt, 0, 10, 0));
			this.NETToDBTypeConversions.Add(new NETToDBTypeConversion(typeof(UInt16), (int)AsaDbTypes.UnsignedSmallInt, 0, 5, 0));
		}


		/// <summary>
		/// Fills the DB type sort order list.
		/// </summary>
		protected override void FillDBTypeSortOrderList()
		{
			// byte[]
			this.SortOrderPerDBType.Add((int)AsaDbTypes.LongBinary, 0);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Image, 0);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.VarBinary, 1);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Binary, 2);

			// string
			this.SortOrderPerDBType.Add((int)AsaDbTypes.NText, 0);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Text, 1);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.LongNVarChar, 1);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.LongVarChar, 3);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Xml, 4);

			this.SortOrderPerDBType.Add((int)AsaDbTypes.NVarChar, 0);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.VarChar, 1);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.NChar, 2);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Char, 3);

			// datetime
			this.SortOrderPerDBType.Add((int)AsaDbTypes.DateTime, 0);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Date, 1);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.SmallDateTime, 2);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.TimeStamp, 3);

			// decimal
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Decimal, 0);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.SmallMoney, 1);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Money, 2);
			this.SortOrderPerDBType.Add((int)AsaDbTypes.Numeric, 3);
		}


		/// <summary>
		/// Gets the decimal types for this driver. Used for optimizing the SortOrder per db type.
		/// </summary>
		/// <returns>
		/// List with the db type values requested or an empty list if not applicable
		/// </returns>
		protected override List<int> GetDecimalTypes()
		{
			return new List<int> { (int)AsaDbTypes.Decimal, (int)AsaDbTypes.Numeric };
		}

		/// <summary>
		/// Gets the currency types for this driver. Used for optimizing the SortOrder per db type.
		/// </summary>
		/// <returns>
		/// List with the db type values requested or an empty list if not applicable
		/// </returns>
		protected override List<int> GetCurrencyTypes()
		{
			return new List<int> { (int)AsaDbTypes.SmallMoney, (int)AsaDbTypes.Money};
		}

		/// <summary>
		/// Gets the fixed length types (with multiple bytes, like char, binary), not b/clobs, for this driver. Used for optimizing the SortOrder per db type.
		/// </summary>
		/// <returns>
		/// List with the db type values requested or an empty list if not applicable
		/// </returns>
		/// <remarks>It's essential that natural character types are stored at a lower index than normal character types.</remarks>
		protected override List<int> GetFixedLengthTypes()
		{
			return new List<int> { (int)AsaDbTypes.NChar, (int)AsaDbTypes.Char, (int)AsaDbTypes.Binary };
		}

		/// <summary>
		/// Gets the variable length types (with multiple bytes, like varchar, varbinary), not b/clobs, for this driver. Used for optimizing the SortOrder per
		/// db type.
		/// </summary>
		/// <returns>
		/// List with the db type values requested or an empty list if not applicable
		/// </returns>
		/// <remarks>It's essential that natural character types are stored at a lower index than normal character types.</remarks>
		protected override List<int> GetVariableLengthTypes()
		{
			return new List<int> { (int)AsaDbTypes.NVarChar, (int)AsaDbTypes.VarChar, (int)AsaDbTypes.VarBinary};
		}


		/// <summary>
		/// Returns the DBType value related to the type passed in in stringformat. The string is read
		/// from SybaseAsa. This routine is not part of IDBDrver, but a routine helping IDBSchema routines.
		/// </summary>
		/// <param name="SybaseAsaType">SybaseAsa type in stringformat. Casing is not important.</param>
		/// <param name="lengthInBytes">The length in bytes. Used for floats</param>
		/// <returns>
		/// DBType value representing the same type as SybaseAsaType
		/// </returns>
		internal static int ConvertStringToDBType(string SybaseAsaType, int lengthInBytes)
		{
			int toReturn;

			switch(SybaseAsaType.ToLowerInvariant())
			{
				case "bigint":
					toReturn = (int)AsaDbTypes.BigInt;
					break;
				case "binary":
					toReturn = (int)AsaDbTypes.Binary;
					break;
				case "bit":
					toReturn = (int)AsaDbTypes.Bit;
					break;
				case "char":
					toReturn = (int)AsaDbTypes.Char;
					break;
				case "date":
					toReturn = (int)AsaDbTypes.Date;
					break;
				case "datetime":
					toReturn = (int)AsaDbTypes.DateTime;
					break;
				case "decimal":
					toReturn = (int)AsaDbTypes.Decimal;
					break;
				case "double":
					toReturn = (int)AsaDbTypes.Double;
					break;
				case "float":
					if(lengthInBytes <= 4)
					{
						toReturn = (int)AsaDbTypes.FloatReal;
					}
					else
					{
						toReturn = (int)AsaDbTypes.FloatDouble;
					}
					break;
				case "image":
					toReturn = (int)AsaDbTypes.Image;
					break;
				case "int":
				case "integer":
					toReturn = (int)AsaDbTypes.Integer;
					break;
				case "long binary":
					toReturn = (int)AsaDbTypes.LongBinary;
					break;
				case "long nvarchar":
					toReturn = (int)AsaDbTypes.LongNVarChar;
					break;
				case "long varbit":
					toReturn = (int)AsaDbTypes.LongVarBit;
					break;
				case "long varchar":
					toReturn = (int)AsaDbTypes.LongVarChar;
					break;
				case "money":
					toReturn = (int)AsaDbTypes.Money;
					break;
				case "nchar":
					toReturn = (int)AsaDbTypes.NChar;
					break;
				case "ntext":
					toReturn = (int)AsaDbTypes.NText;
					break;
				case "numeric":
					toReturn = (int)AsaDbTypes.Numeric;
					break;
				case "nvarchar":
					toReturn = (int)AsaDbTypes.NVarChar;
					break;
				case "real":
					toReturn = (int)AsaDbTypes.Real;
					break;
				case "smalldatetime":
					toReturn = (int)AsaDbTypes.SmallDateTime;
					break;
				case "smallint":
					toReturn = (int)AsaDbTypes.SmallInt;
					break;
				case "smallmoney":
					toReturn = (int)AsaDbTypes.SmallMoney;
					break;
				case "text":
					toReturn = (int)AsaDbTypes.Text;
					break;
				case "time":
					toReturn = (int)AsaDbTypes.Time;
					break;
				case "timestamp":
					toReturn = (int)AsaDbTypes.TimeStamp;
					break;
				case "tinyint":
					toReturn = (int)AsaDbTypes.TinyInt;
					break;
				case "uniqueidentifier":
					toReturn = (int)AsaDbTypes.UniqueIdentifier;
					break;
				case "unsigned bigint":
					toReturn = (int)AsaDbTypes.UnsignedBigInt;
					break;
				case "unsigned int":
					toReturn = (int)AsaDbTypes.UnsignedInt;
					break;
				case "unsigned smallint":
					toReturn = (int)AsaDbTypes.UnsignedSmallInt;
					break;
				case "varbinary":
					toReturn = (int)AsaDbTypes.VarBinary;
					break;
				case "varbit":
					toReturn = (int)AsaDbTypes.VarBit;
					break;
				case "varchar":
					toReturn = (int)AsaDbTypes.VarChar;
					break;
				case "xml":
					toReturn = (int)AsaDbTypes.Xml;
					break;
				default:
					toReturn = (int)AsaDbTypes.VarChar;
					break;
			}

			return toReturn;
		}


		/// <summary>
		/// Returns true if the passed in type is a numeric type. This method is used to determine if a field can be filled with a sequence value.
		/// </summary>
		/// <param name="dbType">type to check</param>
		/// <returns>true if the type is a numeric type, false otherwise</returns>
		public override bool DBTypeIsNumeric(int dbType)
		{
			bool toReturn = false;

			switch((AsaDbTypes)dbType)
			{
				case AsaDbTypes.BigInt:
				case AsaDbTypes.Decimal:
				case AsaDbTypes.Double:
				case AsaDbTypes.FloatDouble:
				case AsaDbTypes.FloatReal:
				case AsaDbTypes.Integer:
				case AsaDbTypes.Money:
				case AsaDbTypes.Numeric:
				case AsaDbTypes.Real:
				case AsaDbTypes.SmallInt:
				case AsaDbTypes.SmallMoney:
				case AsaDbTypes.TinyInt:
				case AsaDbTypes.UnsignedBigInt:
				case AsaDbTypes.UnsignedInt:
				case AsaDbTypes.UnsignedSmallInt:
					toReturn = true;
					break;
			}

			return toReturn;
		}


		/// <summary>
		/// Creates the connectiondata object to be used to obtain the required information for connecting to the database.
		/// </summary>
		/// <returns></returns>
		public override ConnectionDataBase CreateConnectionDataCollector()
		{
			return new SybaseAsaConnectionData(this);
		}


		/// <summary>
		/// Produces the DBCatalogRetriever instance to use for retrieving meta-data of a catalog.
		/// </summary>
		/// <returns>ready to use catalog retriever object</returns>
		public override DBCatalogRetriever CreateCatalogRetriever()
		{
			return new SybaseAsaCatalogRetriever(this);
		}


		/// <summary>
		/// Gets all schema names from the catalog with the name specified in the database system connected through the specified connection elements set.
		/// </summary>
		/// <param name="catalogName">Name of the catalog.</param>
		/// <returns>
		/// List of all schema names in the catalog specified. By default it returns a list with 'Default' for systems which don't use schemas.
		/// </returns>
		public override List<string> GetAllSchemaNames(string catalogName)
		{
			DbDataAdapter adapter = this.CreateDataAdapter("select name as SchemaName from sysusers where name not in ('SYS', 'dbo', 'PUBLIC', 'rs_systabgroup', 'SA_DEBUG')");
			DataTable schemaRows = new DataTable();
			adapter.Fill(schemaRows);
			return (from row in schemaRows.AsEnumerable()
					select row.Value<string>("SchemaName")).ToList();
		}


		/// <summary>
		/// Gets all table names in the schema in the catalog specified in the database system connected through the specified connection elements set.
		/// </summary>
		/// <param name="catalogName">Name of the catalog.</param>
		/// <param name="schemaName">Name of the schema.</param>
		/// <returns>
		/// List of all the table names (not synonyms) in the schema in the catalog specified. By default it returns an empty list.
		/// </returns>
		public override List<DBElementName> GetAllTableNames(string catalogName, string schemaName)
		{
			return GetAllElementNames(string.Format("select table_name as ElementName, r.remarks from systab left join sysremark r on systab.object_id = r.object_id where table_type = 1 and creator = ( select user_id from sysuser where user_name='{0}')", schemaName));
		}


		/// <summary>
		/// Gets all view names in the schema in the catalog specified in the database system connected through the specified connection elements set.
		/// </summary>
		/// <param name="catalogName">Name of the catalog.</param>
		/// <param name="schemaName">Name of the schema.</param>
		/// <returns>
		/// List of all the view names (synonyms) in the schema in the catalog specified. By default it returns an empty list.
		/// </returns>
		public override List<DBElementName> GetAllViewNames(string catalogName, string schemaName)
		{
			return GetAllElementNames(string.Format("select table_name as ElementName, r.remarks from systab left join sysremark r on systab.object_id = r.object_id where table_type in (2, 21) and creator = ( select user_id from sysuser where user_name='{0}')", schemaName));
		}


		/// <summary>
		/// Gets all stored procedure names in the schema in the catalog specified in the database system connected through the specified connection elements set.
		/// </summary>
		/// <param name="catalogName">Name of the catalog.</param>
		/// <param name="schemaName">Name of the schema.</param>
		/// <returns>
		/// List of all the stored procedure names in the schema in the catalog specified. By default it returns an empty list.
		/// </returns>
		public override List<DBElementName> GetAllStoredProcedureNames(string catalogName, string schemaName)
		{
			return GetAllElementNames(string.Format("select proc_name As ElementName, remarks from sysprocedure where creator = (select user_id from sysuser where user_name='{0}')", schemaName));
		}


		/// <summary>
		/// Gets all system sequence instances for the database targeted. System sequences are sequences which are system wide, like @@IDENTITY.
		/// </summary>
		/// <returns>
		/// List of system sequences for this database. By default it returns an empty list
		/// </returns>
		/// <remarks>Method is expected to produce these sequences without a database connection.</remarks>
		public override List<DBSequence> GetAllSystemSequences()
		{
			return new List<DBSequence> { new DBSequence("@@IDENTITY") };
		}


		/// <summary>
		/// Gets the target description of the target the driver is connected to, for display in a UI
		/// </summary>
		/// <returns>
		/// string usable to display in a UI which contains a description of the target the driver is connected to.
		/// </returns>
		public override string GetTargetDescription()
		{
			return string.Format("{0} (Server: {1}. Version: {2}.)", this.DBDriverType, this.ConnectionElements[ConnectionElement.ServerName], this.ServerVersion);
		}


		/// <summary>
		/// Constructs a valid connection string from the elements specified in the hashtable connectionElements.
		/// </summary>
		/// <param name="connectionElementsToUse">The connection elements to use when producing the connection string</param>
		/// <returns>
		/// A valid connection string which is usable to connect to the database to work with.
		/// </returns>
		public override string ConstructConnectionString(Dictionary<ConnectionElement, string> connectionElementsToUse)
		{
			return string.Format("UserID={0};Password={1};DatabaseName={2};ServerName={3};CommLinks=TCPIP()",
					(connectionElementsToUse.GetValue(ConnectionElement.UserID) ?? string.Empty).Replace(";", "';'"),
					(connectionElementsToUse.GetValue(ConnectionElement.Password) ?? string.Empty).Replace(";", "';'"),
					connectionElementsToUse.GetValue(ConnectionElement.CatalogName) ?? string.Empty,
					connectionElementsToUse.GetValue(ConnectionElement.ServerName) ?? string.Empty);
		}


		/// <summary>
		/// Gets the DbProviderFactory invariant names to use for the factory. The first one which is found is used.
		/// </summary>
		/// <returns>list of invariant names</returns>
		protected override List<string> GetDbProviderFactoryInvariantNames()
		{
			return new List<string> { "iAnywhere.Data.SQLAnywhere", "Sap.Data.SQLAnywhere" };
		}


		/// <summary>
		/// Gets all element names using the query specified. For table/view names
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns></returns>
		private List<DBElementName> GetAllElementNames(string query)
		{
			DbDataAdapter adapter = this.CreateDataAdapter(query);
			DataTable tableNames = new DataTable();
			adapter.Fill(tableNames);
			return (from row in tableNames.AsEnumerable()
					select new DBElementName(row.Value<string>("ElementName"), row.Value<string>("remarks") ?? string.Empty)).ToList();
		}
	}
}