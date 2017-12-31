using DynamicPowerShellApi.Configuration;

namespace DynamicPowerShellApi
{
	/// <summary>	A constants helper class. </summary>
	public static class Constants
	{
        /// <summary>	
        /// 	Full pathname of OpenApi specification URL file. 
        /// </summary>
        public const string SpecificationUrlPath = "/api/server/spec";

        /// <summary>	
        /// 	Full pathname of the status URL file. 
        /// </summary>
        public const string StatusUrlPath = "/api/server/status";

		/// <summary>	Full pathname of the job list file. </summary>
		public const string JobListPath = "/api/server/jobs";

		/// <summary>	Full URI of the get job file. </summary>
		public const string GetJobPath = "/api/server/job";

        /// <summary>	Default location of undefined parameters in config file. </summary>
        public const RestLocation DefaultParameterLocation = RestLocation.Body;

        /// <summary>	Default RET Method of undefined parameters in config file. </summary>
        public const RestMethod DefaultRestMethod = RestMethod.Get;

    }
}
