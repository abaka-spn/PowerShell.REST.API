using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;

namespace DynamicPowerShellApi.Configuration
{
    public class PSCommand
    {
        //Name of command
        public string Name { get; set; }

        //Name of the api on which it depends
        public string ApiName { get; set; }

        //Synopsis of Powershell command (from Powershell Help)
        public string Synopsis { get; set; }

        //Synopsis of Powershell command (from Powershell Help)
        public string Description { get; set; }

        //Name of PowerShell Command to will be executed
        public string CommandName { get; set; }

        // Module name of Command
        public string ModuleName { get; set; }

        //Noun Name of PowerShell Command (w/o verb)
        //public string Noun { get; set; }

        public string[] Links { get; set; }

        //Rest method recommended to use
        public RestMethod RestMethod;

        public string[] OutTypeName { get; set; }


        public List<PSParameter> Parameters { get; set; } = new List<PSParameter>();


        /// <summary>
        /// Gets all parameters defined in config file.
        /// </summary>
        public List<PSParameter> GetParametersRequired()
        {
            return Parameters.Where(x => x.Required)
                             .OrderBy(x => x.Position)
                             .ToList();
        }

        /// <summary>
        /// Gets all parameters for specific location.
        /// </summary>
        public List<PSParameter> GetParametersByLocation(RestLocation location)
        {
            return Parameters.Where(x => x.Location == RestLocation.Path)
                             .OrderBy(x => x.Position)
                             .ToList();
        }



        public string GetRoutePath(string rootApiPath)
        {
            string route = string.Format("{0}/{1}/{2}", rootApiPath, ApiName, CommandName);

            Parameters.Where(x => x.Location == RestLocation.Path)
                      .OrderBy(x => x.Position)
                      .Select(x => x.Name)
                      .ToList()
                      .ForEach(x => route += "/{" + x + "}");

            return route;
        }

        public string GetQueryString()
        {
            var paz = Parameters.Where(x => x.Location == RestLocation.Query)
                                .OrderBy(x => x.Position)
                                .Select(x => x.Name + "={" + x.TypeName + "}")
                                .ToArray();

            if (paz.Length > 0)
                return "?" + string.Join("&", paz);
            else
                return "";
        }

        public string GetRequestContentType()
        {
            var paz = Parameters.Where(x => x.Location == RestLocation.Body).Select(x => x.JsonType).ToArray();

            if (paz.Length == 0)
                return "text/plain";
            else if (paz.Length == 1 && (paz[0] == JSchemaType.Array || paz[0] == JSchemaType.Object))
                return "text/plain";
            else
                return "application/json";
        }
    }
}
