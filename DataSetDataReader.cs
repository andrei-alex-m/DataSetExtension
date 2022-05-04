using System;
using System.Diagnostics;
using FCLData = System.Data;
using Microsoft.ReportingServices.DataProcessing;

namespace DataSourceExtension
{
	/// <summary>
	/// Summary description for DataSetDataReader.
	/// </summary>
	public class DataSetDataReader : IDataReader
	{

		#region Constructors

		internal DataSetDataReader(DataSetCommand command)
		{
			//set member variables based upon command object
			dataSetCommand = command;
			//this.dataSet = command.dataSet;
			dataView = command.dataView;
			//this.tableName = command.tableName;

		}

		
		#endregion
		
		#region Member Variables

	    readonly FCLData.DataView dataView;
	    readonly DataSetCommand dataSetCommand = null;
		int currentRow= -1;
		
		#endregion

		#region IDataReader Members

		#region GetValue
		/// <summary>
		/// Gets the value of a field from the current row based upon its ordinal position
		/// </summary>
		/// <param name="fieldIndex">The index of the field</param>
		/// <returns>The value of the column</returns>
		public object GetValue (int fieldIndex)
		{
			Debug.WriteLine("IDataReader.GetValue");
			return( dataView [currentRow] [fieldIndex]);
		}
		#endregion

		#region GetFieldType
		/// <summary>
		/// Retrieves the data type of a field
		/// </summary>
		/// <param name="fieldIndex">The index of the field</param>
		/// <returns>The data type of the specified field</returns>
		public Type GetFieldType (int fieldIndex)
		{
			Debug.WriteLine("IDataReader.GetFieldType");
			return( dataView.Table.Columns[fieldIndex].DataType);
		}
		#endregion

		#region GetName
		/// <summary>
		/// Retrieves the name of a field by on its ordinal position
		/// </summary>
		/// <param name="fieldIndex">The index of the field</param>
		/// <returns>The name of the specified field</returns>
		public String GetName (int fieldIndex)
		{
			 return (dataView.Table.Columns[fieldIndex].ColumnName);
		}

		#endregion

		#region FieldCount
		/// <summary>
		/// Retrieves the number of fields per row
		/// </summary>
		public int FieldCount
		{
			get
			{
				Debug.WriteLine("IDataReader.FieldCount: Get");
				return (dataView.Table.Columns.Count);
			}
		}

	    public DataSetCommand DataSetCommand
	    {
	        get { return dataSetCommand; }
	    }

	    #endregion

		#region GetOrdinal
		/// <summary>
		/// Retrieves the ordinal position of a field based on the field name
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>The ordinal of the specified field</returns>
		public int GetOrdinal (String fieldName)
		{
				return( dataView.Table.Columns[fieldName].Ordinal);
		}
		#endregion

		#region Read
		/// <summary>
		/// Moves to the first or next row
		/// </summary>
		/// <returns>Returns true until no more data is present</returns>
		public Boolean Read ()
		{
			//MoveNext Row
			currentRow ++;
			if (currentRow >= dataView.Count)
			{
				// Finished Reading
				return (false);
			}
			// Not Finished Reading
			return (true);
		}
		#endregion

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			
		}

		#endregion
	}
}
