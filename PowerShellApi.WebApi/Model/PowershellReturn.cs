﻿namespace PowerShellRestApi.Model
{
	/// <summary>
	/// This class holds the details for what is returned from power shell
	/// </summary>
	public class PowershellReturn
	{
		/// <summary>
		/// This indicates what was returned was valid
		/// </summary>
		public bool PowerShellReturnedValidData { get; set; }

		/// <summary>
		/// This is the value returned
		/// </summary>
		public string ActualPowerShellData { get; set;  }
	}
}