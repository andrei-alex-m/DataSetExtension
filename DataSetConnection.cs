using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.ReportingServices.DataProcessing;
using FCLData = System.Data;



namespace DataSourceExtension
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class DataSetConnection : IDbConnectionExtension
    {

        #region Constructors

        public DataSetConnection()
        {
            Debug.WriteLine("DataSetConnection: Default Constructor");
        }

        public DataSetConnection(string connectionString)
        {
            Debug.WriteLine("DataSetConnection Constructor overloaded with Connection String ");
            ConnectionString = connectionString;
        }

        #endregion

        #region IDbConnection Members

        #region BeginTransaction
        /// <summary>
        /// This method is not implemented because of the read only nature of a 
        /// Reporting Services Data Extension. 
        /// </summary>
        /// <returns>NotImplemented Exception</returns>
        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Close
        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Close()
        {
            Debug.WriteLine("IDBConnection.Close()");
            dataSet = null;
        }
        #endregion

        #region ConnectionString
        /// <summary>
        /// Gets/sets the connection string
        /// </summary>
        /// <remarks>
        /// String must include "FileName=<filename>"
        /// </remarks>
        public String ConnectionString
        {
            get
            {
                return (m_connectionString);
            }
            set
            {
                Debug.WriteLine("Setting IDBConnection.ConnectionString  to '" + value + "'");
                m_connectionString = value;
                Match m = Regex.Match(value, "FileName=([^;]+)", RegexOptions.IgnoreCase);
                if (!m.Success)
                {
                    throw (new ArgumentException("'FileNamed=<filename>' must be present in the connection string and point to a valid DataSet xml file", m_connectionString));
                }
                if (!File.Exists(m.Groups[1].Captures[0].ToString()))
                {
                    throw (new ArgumentException("Incorrect Filename", m_connectionString));
                }
                m_fileName = m.Groups[1].Captures[0].ToString();
            }
        }
        #endregion

        #region ConnectionTimeout - Done
        /// <summary>
        /// Gets the connection timeout.
        /// </summary>
        /// <remarks>
        /// Timeouts not supported - returns 0 indicating unlimited
        /// </remarks>
        public int ConnectionTimeout
        {
            get
            {
                return 0;
            }
        }
        #endregion

        #region CreateCommand
        /// <summary>
        /// Creates a IDbCommand object
        /// </summary>
        /// <returns>A reference to an IDbCommand object</returns>
        public IDbCommand CreateCommand()
        {
            return (new DataSetCommand(this));
        }
        #endregion

        #region Open
        /// <summary>
        /// Opens the connection
        /// </summary>
        public void Open()
        {
            Debug.WriteLine("IDBConnection.Open");
            dataSet = new FCLData.DataSet();
            dataSet.ReadXml(m_fileName);
        }
        #endregion
        #endregion

        #region IDisposable Members - Done

        public void Dispose()
        {
            // TODO:  Add DataSetConnection.Dispose implementation
        }

        #endregion

        #region IExtension Members - Done

        public string LocalizedName
        {
            get
            {
                //TODO: Test for Current culture and return appropriate string
                return m_localizedName;
            }
        }


        public void SetConfiguration(string configuration)
        {
            // TODO:  Add DataSetConnection.SetConfiguration implementation
        }


        #endregion

        #region IDbConnectionExtension Members

        public string Impersonate
        {
            set { m_impersonate = value; }
        }

        public bool IntegratedSecurity
        {
            get
            {
                return m_integrated;
            }
            set
            {
                m_integrated = value;
            }
        }

        public string Password
        {
            set { m_password = value; }
        }

        public string UserName
        {
            set { m_userName = value; }
        }

        public string M_userName
        {
            get { return m_userName; }
        }

        public string M_password
        {
            get { return m_password; }
        }

        public string M_impersonate
        {
            get { return m_impersonate; }
        }

        #endregion

        private string m_userName;
        private string m_password;
        private bool m_integrated;
        private string m_impersonate;
        private string m_connectionString = String.Empty;
        private readonly string m_localizedName = "Custom DataSource Extension";
        private string m_fileName;

        internal FCLData.DataSet dataSet;

    }

}
