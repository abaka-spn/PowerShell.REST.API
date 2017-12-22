using System;

namespace DynamicPowerShellApi.Configuration
{
    public class ApiPath
    {
        public static string Application
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }


        public static string ScriptRepository
        {
            get
            {
                return System.IO.Path.Combine(Application, "ScriptRepository");
            }
        }
    }
}
