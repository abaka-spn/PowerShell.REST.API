using System.Configuration;

namespace Mee.Owin.Security.ApiKey.Config
{
    /// <summary>
    /// A set of <see cref="ApiKey.User"/>s for HTTP Basic Authentication.
    /// </summary>
    public class UserCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new User();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((User)element).Name;
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName == "users";
        }
    }
}