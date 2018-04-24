using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace PowerShellRestApi.PSConfiguration
{
    public class PSModel
    {
        #region Static definitions

        /// <summary>
        /// Private singleton to store all models
        /// </summary>
        private static readonly Lazy<Dictionary<string, PSModel>> _lazyPSModels = new Lazy<Dictionary<string, PSModel>>(() => new Dictionary<string, PSModel>());

        /// <summary>
        /// Public instance to retrieve all models
        /// </summary>
        public static Dictionary<string, PSModel> AllModels { get { return _lazyPSModels.Value; } }

        public static PSModel AddModel (Type ElementType)
        {
            if (PSModel.AllModels.ContainsKey(ElementType.Name))
                return PSModel.AllModels[ElementType.Name];

            var psModel = new PSModel(ElementType.Name);

            // Add to dictionary now to avoid loops
            AllModels[ElementType.Name] = psModel;

            foreach (var pro in ElementType.GetProperties())
            {
                var proName = pro.Name;

                var psParam = new PSParameter(pro.Name, pro.PropertyType);
                psParam.GenerateModel();

                psParam.AddValidate(pro.GetCustomAttributes(true));

                // Add parameter to dictionary
                psModel.Parameters.Add(psParam);
            }


            if (psModel.Parameters.Count != 0)
            {
                return psModel;
            }
            else
            {
                return null;
            }

        }
        #endregion  



        /// <summary>
        /// Model Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// List of parameters for this command
        /// </summary>
        public List<PSParameter> Parameters { get; set; } = new List<PSParameter>();


        /// <summary>
        /// Gets all parameters defined in config file.
        /// </summary>
        public List<PSParameter> GetParametersRequired()
        {
            return Parameters.Where(x => x.Required && !x.Hidden)
                             .OrderBy(x => x.Position)
                             .ToList();
        }

        /// <summary>
        /// Gets all parameters for specific location.
        /// </summary>
        public List<PSParameter> GetParametersByLocation(RestLocation location)
        {
            return Parameters.Where(x => x.Location == location && !x.Hidden)
                             .OrderBy(x => x.Position)
                             .ToList();
        }

        /// <summary>
        /// Gets all parameters execpt for specific location.
        /// </summary>
        public List<PSParameter> GetParametersExceptForLocation(RestLocation location)
        {
            return Parameters.Where(x => x.Location != location && !x.Hidden)
                             .OrderBy(x => x.Position)
                             .ToList();
        }

        public PSModel(string Name)
        {
            this.Name = Name;
        }

        public OpenApiSchema GetOpenApiSchema()
        {
            var openApiProperties = new Dictionary<string, OpenApiSchema>();
            var required = new List<string>();
            foreach (var apiParameter in this.Parameters.Where(x => x.Location == RestLocation.Body && !x.Hidden))
            {
                openApiProperties.Add
                (
                    apiParameter.Name,
                    apiParameter.GetOpenApiSchema()
                );

                if (apiParameter.Required)
                    required.Add(apiParameter.Name);
            }

            return new OpenApiSchema() { Type = "object", Properties = openApiProperties, Required = required };
        }

    }
}
