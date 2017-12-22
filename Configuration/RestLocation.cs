using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPowerShellApi.Configuration
{
    public enum RestLocation
    {
        // Same values as Microsoft.OpenApi.Models.ParameterLocation
        // Body value added but need specfic transformation


        /// <summary>
        /// Parameters that are appended to the URL.
        /// </summary>
        Query,

        /// <summary>
        /// Custom headers that are expected as part of the request.
        /// </summary>
        Header,

        /// <summary>
        /// Used together with Path Templating,
        /// where the parameter value is actually part of the operation's URL
        /// </summary>
        Path,

        /// <summary>
        /// Used to pass a specific cookie value to the API.
        /// </summary>
        Cookie,

        /// <summary>
        /// Used together with Body
        /// </summary>
        Body

    }

}
