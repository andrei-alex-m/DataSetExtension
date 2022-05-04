using System.Collections;

using Microsoft.ReportingServices.DataProcessing;

namespace DataSourceExtension
{
	/// <summary>
	/// Summary description for DataSetParameterCollection.
	/// </summary>
	public class DataSetParameterCollection : IDataParameterCollection
	{			

		#region Constructors

		public DataSetParameterCollection()
		{
			paramList = new ArrayList();
		}


		#endregion

		#region Member Variables 

	    readonly ArrayList paramList;

		#endregion

		#region IDataParameterCollection Members - Done

		#region Add
		/// <summary>
		/// Adds a new parameter
		/// </summary>
		/// <param name="parameter">Parameter Object</param>
		/// <returns>ordinal position of inserted parameter</returns>
		public int Add (IDataParameter parameter)
		{
			return (paramList.Add (parameter));
		}
		#endregion

		#region IEnumerable Members

		#region GetEnumerator
		/// <summary>
		/// Retrieves an enumerator for the parameters
		/// </summary>
		/// <returns>Interface used for object enumeration</returns>
		public IEnumerator GetEnumerator ()
		{
			return (paramList.GetEnumerator ());
		}
		#endregion
		
		#endregion

		#endregion
	}
}
