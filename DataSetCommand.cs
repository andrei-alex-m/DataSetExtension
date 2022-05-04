using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using FCLData = System.Data;
using Microsoft.ReportingServices.DataProcessing;

//https://www.codeproject.com/Articles/22946/Implementing-a-Data-Processing-Extension
//https://docs.microsoft.com/en-us/sql/reporting-services/extensions/data-processing/implementing-a-command-class-for-a-data-processing-extension?view=sql-server-ver15
namespace DataSourceExtension
{

    /// <summary>
    /// Summary description for DataSetCommand.
    /// </summary>
    public class DataSetCommand : IDbCommand
    {
        #region Member Variables
        //member variables
        int m_commandTimeOut = 0;
        string m_commandText = String.Empty;
        readonly DataSetConnection m_connection;
        readonly DataSetParameterCollection m_parameters;
        //dataset variables
        string tableName = String.Empty;
        readonly FCLData.DataSet dataSet = null;
        internal FCLData.DataView dataView = null;
        //regex variables
        MatchCollection kwc = null;
        Match fieldMatch = null;
        //regex used for getting keywords
        readonly Regex keywordSplit = new Regex(@"(Select|From|Where| Order[ \s] +By)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline
            | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        // regex used for spliting out fields
        readonly Regex fieldSplit = new Regex(@"([^ ,\s]+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline
            | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        //internal variables
        int keyWordCount = 0;
        const int selectPosition = 0;
        const int fromPosition = 1;
        //these can change
        readonly int wherePosition = 2;
        int orderPosition = 3;
        const string tempTableName = "TempTable";

        bool filtering = false;
        bool sorting = false;
        bool useDefaultTable = false;

        #endregion

        #region Constructors

        internal DataSetCommand(DataSetConnection conn)
        {
            Debug.WriteLine("Command : Entering  constructor DataSetCommand(DataSetConnection conn)");
            m_connection = conn;
            dataSet = m_connection.dataSet;
            m_parameters = new DataSetParameterCollection();
        }
        #endregion

        #region IDbCommand Members

        #region Cancel
        /// <summary>
        /// Attempts to cancel the current command
        /// </summary>
        public void Cancel()
        {
            Debug.WriteLine("IDBCommand.Cancel");
            throw (new NotSupportedException("IDBCommand.Cancel currently not supported"));
        }
        #endregion

        #region CommandType
        /// <summary>
        /// Gets/sets the current command type
        /// </summary>
        /// <remarks>
        /// Only the Text command type is supported
        /// </remarks>

        public CommandType CommandType
        {
            get
            {
                Debug.WriteLine("IDBCommand.CommandType: Get : ");
                return (CommandType.Text);
            }
            set
            {
                Debug.WriteLine("IDBCommand.CommandType: Set");
                if (value != CommandType.Text)
                {
                    throw (new NotSupportedException("Only CommandType.Text is supported."));
                }
            }
        }
        #endregion

        #region ExecuteReader
        /// <summary>
        /// Retrieves IDataReader interface used to retrieve data and schema information.
        /// </summary>
        /// <param name="behavior">The requested  command behavior behavior</param>
        /// <returns>IDataReader Interface</returns>
        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            Debug.WriteLine("IDBCommand.ExecuteReader with CommandBehavior." + behavior);

            if (!(behavior == CommandBehavior.SchemaOnly) && !useDefaultTable)
            {
                FillView();
            }
            return new DataSetDataReader(this);
        }
        #endregion

        #region CommandTimeout
        /// <summary>
        /// Gets/sets the command timeout.
        /// </summary>
        /// <remarks>
        /// Gets or Sets the Timeout Value
        /// </remarks>
        public int CommandTimeout
        {
            get
            {
                Debug.WriteLine("IDBCommand.CommandTimeout: Get");
                return m_commandTimeOut;
            }
            set
            {
                m_commandTimeOut = value;
                Debug.WriteLine("IDBCommand.CommandTimeout: Set");
                //throw new NotImplementedException("Times outs not supported");
            }
        }
        #endregion

        #region CreateParameter
        /// <summary>
        /// Creates a new data parameter
        /// </summary>
        /// <returns>The new data parameter</returns>
        public IDataParameter CreateParameter()
        {
            Debug.WriteLine("IDBCommand.CreateParameter");
            return (new DataSetParameter());
        }
        #endregion

        #region CommandText
        /// <summary>
        /// Gets/sets the current command text
        /// </summary>
        /// <remarks>
        /// CommandText must be in the format of:
        /// select from <table> [where <filter>] [order by <column> [asc | desc]]
        /// </remarks>
        public String CommandText
        {
            get
            {
                Debug.WriteLine("IDBCommand.CommandText: Get Value =" + m_commandText);
                return (m_commandText);
            }
            set
            {
                Debug.WriteLine("IDBCommand.CommandText: Set Value =" + value);
                ValidateCommandText(value);
                m_commandText = value;
            }
        }
        #endregion

        #region Transaction - Done
        /// <summary>
        ///Transaction Object - Not Supported
        /// </summary>
        /// <returns> Not Supported</returns>
        public IDbTransaction Transaction
        {
            get
            {
                throw (new NotSupportedException("Transactions not supported"));
            }
            set
            {
                throw (new NotSupportedException("Transactions not supported"));
            }
        }
        #endregion

        #region Parameters - Done
        /// <summary>
        /// Retreives the current parameters list
        /// </summary>
        public IDataParameterCollection Parameters
        {
            get
            {
                Debug.WriteLine("IDBCommand: Retrieving parameters list");
                return m_parameters;
            }
        }
        #endregion
        #endregion

        #region IDisposable Members
        #region Dispose
        public void Dispose()
        {
            Debug.WriteLine("Disposing");
        }
        #endregion
        #endregion

        #region Validate Command Text
        private void ValidateCommandText(string cmdText)
        {
            kwc = keywordSplit.Matches(cmdText);
            keyWordCount = kwc.Count;
            switch (keyWordCount)
            {
                case 4:
                    sorting = true;
                    filtering = true;
                    break;
                case 3:
                    if (kwc[keyWordCount - 1].ToString().ToUpper() == "WHERE")
                        filtering = true;
                    else
                    {
                        sorting = true;
                        orderPosition = 2;
                    }
                    break;
                case 2:
                    break;
                default:
                    throw (new ArgumentException("Command Text should start with 'select <fields> from <tablename>'"));
            }

            ValidateTableName(cmdText);
            ValidateFieldNames(cmdText);
            if (filtering)
            {
                ValidateFiltering(cmdText);
            }
            if (sorting)
            {
                ValidateSorting(cmdText);
            }
        }
        #endregion

        #region Validate Table Name
        private void ValidateTableName(string cmdText)
        {
            //Get tablename
            //get 1st match starting at end of from
            fieldMatch = fieldSplit.Match(cmdText,
                (kwc[fromPosition].Index) + kwc[fromPosition].Length + 1);
            if (fieldMatch.Success)
            {
                if (dataSet.Tables.Contains(fieldMatch.Value))
                {
                    tableName = fieldMatch.Value;
                }
                else
                {
                    throw new ArgumentException("Invalid Table Name");
                }
            }
        }
        #endregion

        #region Validate FieldNames
        public void ValidateFieldNames(string cmdText)
        {
            //get fieldnames 
            //get first match starting at the last character of the Select
            // with a length from that position to the from
            fieldMatch = fieldSplit.Match(cmdText,
                (kwc[selectPosition].Index + kwc[selectPosition].Length + 1),
                (kwc[fromPosition].Index - (kwc[selectPosition].Index + kwc[selectPosition].Length + 1)));

            if (fieldMatch.Value == "*")  // all fields, use default view
            {
                dataView = dataSet.Tables[tableName].DefaultView;
                useDefaultTable = true;
            }
            else   //custom fields :  must build table/view
            {
                useDefaultTable = false; //don't use default table
                //remove table if exists - add new
                if (dataSet.Tables.Contains(tempTableName))
                {
                    dataSet.Tables.Remove(tempTableName);
                }
                FCLData.DataTable table = new FCLData.DataTable(tempTableName);
                //loop through column matches
                while (fieldMatch.Success)
                {
                    if (dataSet.Tables[tableName].Columns.Contains(fieldMatch.Value))
                    {
                        FCLData.DataColumn col = dataSet.Tables[tableName].Columns[fieldMatch.Value];
                        table.Columns.Add(new FCLData.DataColumn(col.ColumnName, col.DataType));
                        fieldMatch = fieldMatch.NextMatch();
                    }
                    else
                    {
                        throw new ArgumentException("Invalid column name");
                    }
                }
                //add temptable to internal dataset and set view to tempView;
                dataSet.Tables.Add(table);
                dataView = new FCLData.DataView(table);
            }
        }
        #endregion

        #region Validate Filtering
        public void ValidateFiltering(string cmdText)
        {
            if (filtering)
            {
                StringBuilder sbFilterText = new StringBuilder();
                int startPos;
                int length;

                startPos = (kwc[wherePosition].Index + kwc[wherePosition].Length + 1);

                if (keyWordCount == 3)  //no "order by" - Search from Where till  end
                {
                    length = cmdText.Length - startPos;
                }
                else // "order by" exists -  search from where  position to "order by"
                {
                    length = kwc[orderPosition].Index - startPos;
                }

                sbFilterText.Append(cmdText.Substring(startPos, length));
                dataView.RowFilter = sbFilterText.ToString();
            }
        }
        #endregion

        #region Validate Sorting
        public void ValidateSorting(string cmdText)
        {
            if (sorting)
            {
                StringBuilder sbFilterText = new StringBuilder();
                int startPos;
                int length;
                //start from end of 'Order by' clause
                startPos = (kwc[orderPosition].Index + kwc[orderPosition].Length + 1);

                length = cmdText.Length - startPos;

                sbFilterText.Append(cmdText.Substring(startPos, length));
                dataView.Sort = sbFilterText.ToString();
            }
        }
        #endregion

        #region FillView

        private void FillView()
        {
            FCLData.DataRow tempRow;
            String[] tempArray;
            int count;

            count = dataSet.Tables[tempTableName].Columns.Count;
            tempArray = new String[count];

            foreach (FCLData.DataRow row in dataSet.Tables[tableName].Rows)
            {
                tempRow = dataSet.Tables[tempTableName].NewRow();

                foreach (FCLData.DataColumn col in dataSet.Tables[tempTableName].Columns)
                {
                    tempArray[col.Ordinal] = row[col.ColumnName].ToString();
                }

                tempRow.ItemArray = tempArray;
                dataSet.Tables[tempTableName].Rows.Add(tempRow);
            }
        }


        #endregion

    } //class
}//namespace