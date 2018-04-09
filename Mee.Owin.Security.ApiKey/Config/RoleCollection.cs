using System.Configuration;

namespace Mee.Owin.Security.ApiKey.Config
{
    /// <summary>
    /// A set of <see cref="Role"/>s for a <see cref="ApiKey.User"/>.
    /// </summary>
    public class RoleCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Role();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Role)element).Name;
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName == "roles";
        }
    }
}