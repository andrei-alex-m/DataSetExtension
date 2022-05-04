using System;
using System.Diagnostics;
using Microsoft.ReportingServices.DataProcessing;

namespace DataSourceExtension
{
	/// <summary>
	/// Summary description for DataSetParameter.
	/// </summary>
	public class DataSetParameter : IDataParameter
	{
		#region Constructors - Done

	    #endregion

		#region Member Variables - Done
		
		string m_parameterName = String.Empty;
		object m_parameterValue;

		#endregion

		#region IDataParameter Members - Done

		#region ParameterName
		/// <summary>
		/// Gets/sets the name of the parameter
		/// </summary>
		public String ParameterName
		{
			get
			{
				Debug.WriteLine ("Getting parameter: " + m_parameterName);
				return (m_parameterName);
			}
			set
			{
				Debug.WriteLine ("Setting parameter: " + value);
				m_parameterName = value;
			}
		}
		#endregion

		#region Value
		/// <summary>
		/// Gets/sets the Value of the parameter
		/// </summary>
		public object Value
		{
			get
			{
				Debug.WriteLine (String.Format ("Getting parameter [{0}] value: [{1}]", m_parameterName, m_parameterValue));
				return (m_parameterValue);
			}
			set
			{
				Debug.WriteLine (String.Format ("Setting parameter [{0}] value: [{1}]", m_parameterName, m_parameterValue));
				m_parameterValue = value;
			}
		}
		#endregion

		#endregion
	}
}
