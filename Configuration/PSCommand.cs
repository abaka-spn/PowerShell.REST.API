using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace DynamicPowerShellApi.Configuration
{
    public class PSCommand : PSModel
    {
        /// <summary>
        /// Name of the api on which it depends
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// Name of command
        /// </summary>
        public string WebMethodName { get; set; }

        /// <summary>	
        /// 	Run this command as a job? I.e. respond with a Job ID. </summary>
        /// <value>	true if as job, false if not. </value>
        public bool AsJob { get; set; }

        /// <summary>
        /// Gets the snap in.
        /// </summary>
        public string Snapin { get; set; }

        //Synopsis of Powershell command (from Powershell Help)
        public string Synopsis { get; set; }

        //Synopsis of Powershell command (from Powershell Help)
        public string Description { get; set; }

        //Name of PowerShell command to will be executed
        //public string Name { get; set; }

        //Module that must be loaded before running the command
        public string Module { get; set; }
        
        // Module name of Command
        public string ModuleName { get; set; }

        //Noun Name of PowerShell Command (w/o verb)
        //public string Noun { get; set; }

        /// <summary>
        /// Links in PowerShell script
        /// </summary>
        public string[] Links { get; set; }

        /// <summary>
        /// Uri to acces at this Command
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Http method will be used to call this command (get, put, post, ...)
        /// </summary>
        public RestMethod RestMethod;

        /// <summary>
        /// Name's type of return value
        /// </summary>
        public string[] OutTypeName { get; set; }

        /// <summary>
        /// Define if Anonymous users are allowed to launch this commmand.
        /// </summary>
        public bool AllowAnonymous { get; set; }

        /// <summary>
        /// Users allowed to launch this commmand.
        /// </summary>
        public string Users { get; set; }

        /// <summary>
        /// Roles allowed to launch this commmand.
        /// </summary>
        public string Roles { get; set; }

  
        /// <summary>
        /// Name of the parameter that will be set with username.
        /// </summary>
        public string ParameterForUser { get; set; }

        /// <summary>
        /// Name of the parameter that will be set with roles list.
        /// </summary>
        public string ParameterForRoles { get; set; }

        /// <summary>
        /// Name of the parameter that will be set with claims list.
        /// </summary>
        public string ParameterForClaims { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name"></param>
        public PSCommand(string Name) : base(Name)
        {
        }

        /*
         * Peut faire plus simple ;-)
        public Claim[] Claims { get; set; } = new Claim[0];

        public bool IsAuthorized(ClaimsIdentity Identity)
        {
            if (!Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.Claims.Length == 0)
                return true;

            return Identity.Claims
                            .FirstOrDefault(id =>
                                            this.Claims
                                                .FirstOrDefault(cmd =>
                                                                cmd.Type == id.Type &&
                                                                cmd.Value == id.Value
                                                                ) != null
                                           ) != null;

            
        }


        public bool IsAuthorized(IPrincipal Principal)
        {
            return this.IsAuthorized(Principal.Identity as ClaimsIdentity);
        }
        */

        /// <summary>
        /// Get url path of this command (first part)
        /// </summary>
        /// <param name="rootApiPath"></param>
        /// <returns></returns>
        public string GetRoutePath()
        {
            string route = Uri.ToString(); // string.Format("{0}/{1}/{2}", rootApiPath, ApiName, Name);

            Parameters.Where(x => x.Location == RestLocation.Path)
                      .OrderBy(x => x.Position)
                      .Select(x => x.Name)
                      .ToList()
                      .ForEach(x => route += "/{" + x + "}");

            return route;
        }

        /// <summary>
        /// Get query string format of this command
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get request content type (text/plain or application/json)
        /// </summary>
        /// <returns></returns>
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
