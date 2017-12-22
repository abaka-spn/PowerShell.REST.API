﻿using DynamicPowerShellApi.Model;

namespace DynamicPowerShellApi
{
	using System.Collections.Generic;
	using System.Threading.Tasks;

	/// <summary>
	/// The Runner interface for executing methods with parameters.
	/// </summary>
	public interface IRunner
	{
		/// <summary>	Asynchronous execution of a method with parameters. </summary>
		/// <param name="filename">		 	The filename. </param>
		/// <param name="snapin">		 	The snap in. </param>
		/// <param name="module">		 	The module. </param>
		/// <param name="parametersList">	The parameters List. </param>
		/// <param name="asJob">		 	Run this as a PowerShell job. </param>
		/// <returns>	The <see cref="Task"/>. </returns>
		Task<PowershellReturn> ExecuteAsync(
			string filename,
			string snapin,
			string module,
			IList<KeyValuePair<string, object>> parametersList,
			bool asJob);
	}
}