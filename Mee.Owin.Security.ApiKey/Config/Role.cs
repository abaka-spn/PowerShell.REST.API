using System.Configuration;

namespace Mee.Owin.Security.ApiKey.Config
{
    /// <summary>
    /// A specific role of a <see cref="ApiKey.User"/>.
    /// </summary>
    public class Role : ConfigurationElement
    {
        /// <summary>
        /// The name of the role.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => (string)base["name"];
    }
}