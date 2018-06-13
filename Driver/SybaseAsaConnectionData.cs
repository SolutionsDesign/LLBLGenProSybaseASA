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
using System.Collections.Generic;
using System.ComponentModel;
using SD.LLBLGen.Pro.DBDriverCore;
using SD.Tools.BCLExtensions.CollectionsRelated;

namespace SD.LLBLGen.Pro.DBDrivers.SybaseAsa
{
	/// <summary>
	/// Data collector for the retrieval of connection information for Sybase ASE style connection strings.
	/// </summary>
	public class SybaseAsaConnectionData : ConnectionDataBase
	{
		#region Members
		private string _serverName, _databaseName, _loginID, _password;
		#endregion

		/// <summary>
		/// CTor
		/// </summary>
		/// <param name="driverToUse">The driver to use for the connection</param>
		public SybaseAsaConnectionData(SybaseAsaDBDriver driverToUse)
			: base(driverToUse)
		{
		}


		/// <summary>
		/// Fills the properties of this object with the data obtained from the ConnectionElements object available in the set driver (if any)
		/// </summary>
		public override void FillProperties()
		{
			if(this.ConnectionElements.Count <= 0)
			{
				return;
			}

			_serverName=this.ConnectionElements.GetValue(ConnectionElement.ServerName) ?? string.Empty;
			_databaseName = this.ConnectionElements.GetValue(ConnectionElement.CatalogName) ?? string.Empty;
			_loginID = this.ConnectionElements.GetValue(ConnectionElement.UserID) ?? string.Empty;
			_password = this.ConnectionElements.GetValue(ConnectionElement.Password) ?? string.Empty;
		}

		
		/// <summary>
		/// Validates the information specified. 
		/// </summary>
		/// <returns>returns true if the information is valid, false otherwise. Caller should not proceed further if false is returned.</returns>
		public override bool ValidateInformation()
		{
			return (!string.IsNullOrWhiteSpace(_serverName) && !string.IsNullOrWhiteSpace(_databaseName) && !string.IsNullOrWhiteSpace(_loginID));
		}


		/// <summary>
		/// Gets the property names for the property bag to bind to the property grid. This list of names is used to create property descriptors
		/// which are easier to read as they have their names split on word breaks.
		/// </summary>
		/// <returns></returns>
		protected override HashSet<string> GetPropertyNamesForBag()
		{
			return new HashSet<string>() { "ServerName", "LoginID", "Password", "DatabaseName" };
		}


		/// <summary>
		/// Constructs a hashtable with the elements for the connection string
		/// </summary>
		/// <returns></returns>
		protected override void ConstructConnectionElementsList()
		{
			this.ConnectionElements.Clear();

			this.ConnectionElements.Add(ConnectionElement.ServerName, _serverName);
			this.ConnectionElements.Add(ConnectionElement.CatalogName, _databaseName);
			this.ConnectionElements.Add(ConnectionElement.UserID, _loginID);
			this.ConnectionElements.Add(ConnectionElement.Password, _password);
		}


		#region Properties
		/// <summary>
		/// Gets or sets the login id.
		/// </summary>
		[Description("The login ID to use when connecting to the server.")]
		[Category("Authentication")]
		public string LoginID
		{
			get { return _loginID; }
			set
			{
				var oldValue = _loginID;
				_loginID = value;
				RaiseOnChange(oldValue, value, "LoginID");
			}
		}


		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		[PasswordPropertyText(true)]
		[Description("The password to use when connecting to the server.")]
		[Category("Authentication")]
		public string Password
		{
			get { return _password; }
			set
			{
				var oldValue = _password;
				_password = value;
				RaiseOnChange(oldValue, value, "Password");
			}
		}


		/// <summary>
		/// Gets or sets the name of the server.
		/// </summary>
		[Description("The name of the SAP SQL Anywhere server to connect to.")]
		[Category("General")]
		public string ServerName
		{
			get { return _serverName; }
			set
			{
				var oldValue = _serverName;
				_serverName = value;
				RaiseOnChange(oldValue, value, "ServerName");
			}
		}


		/// <summary>
		/// Gets or sets the name of the database.
		/// </summary>
		[Description("The SAP SQL Anywhere database to connect to.")]
		[Category("General")]
		public string DatabaseName
		{
			get { return _databaseName; }
			set
			{
				var oldValue = _databaseName;
				_databaseName = value;
				RaiseOnChange(oldValue, _databaseName, "DatabaseName");
			}
		}		
		#endregion
	}
}
