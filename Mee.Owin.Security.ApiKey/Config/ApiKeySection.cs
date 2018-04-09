using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Mee.Owin.Security.ApiKey.Config
{
    public class ApiKeySection : ConfigurationSection
    {
        /// <summary>
        /// The users to accept for authentication.
        /// </summary>
        [ConfigurationProperty("users", IsRequired = true)]
        [ConfigurationCollection(typeof(UserCollection), 
            CollectionType = ConfigurationElementCollectionType.BasicMapAlternate, 
            AddItemName = "user")]
        public UserCollection Users => (UserCollection)base["users"];

        /// <summary>
        /// Loads the configuration from the the application configuration file (Web.config or App.config).
        /// </summary>
        public static ApiKeySection Load()
        {
            return (ApiKeySection)ConfigurationManager.GetSection("apiKey");
        }

    }
}