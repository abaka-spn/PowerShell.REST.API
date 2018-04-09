using System.Configuration;

namespace Mee.Owin.Security.ApiKey.Config
{
    /// <summary>
    /// A specific user for HTTP Basic Authentication.
    /// </summary>
    public class User : ConfigurationElement
    {
        [ConfigurationProperty("apiKey", IsRequired = true)]
        public string ApiKey => (string)base["apiKey"];

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => (string)base["name"];

        [ConfigurationProperty("roles", IsRequired = true)]
        [ConfigurationCollection(typeof(RoleCollection), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate, AddItemName = "role")]
        public RoleCollection Roles => (RoleCollection)base["roles"];
    }
}