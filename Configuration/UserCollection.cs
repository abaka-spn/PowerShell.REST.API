namespace DynamicPowerShellApi.Configuration
{
	using System.Collections.Generic;
	using System.Configuration;

	/// <summary>
	/// The web method collection.
	/// </summary>
	public class UserCollection : ConfigurationElementCollection, IEnumerable<User>
	{
		/// <summary>
		/// Creates a new element.
		/// </summary>
		/// <returns>
		/// The <see cref="ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new User();
		}

		/// <summary>
		/// Gets an element key.
		/// </summary>
		/// <param name="element">
		/// The element.
		/// </param>
		/// <returns>
		/// The <see cref="object"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((User)element).Name;
        }

		/// <summary>
		/// Gets the element name.
		/// </summary>
		protected override string ElementName
		{
			get
			{
				return "User";
			}
		}

		/// <summary>
		/// Is element name?
		/// </summary>
		/// <param name="elementName">
		/// The element name.
		/// </param>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		protected override bool IsElementName(string elementName)
		{
			return !string.IsNullOrEmpty(elementName) && elementName == "User";
		}

		/// <summary>
		/// Gets the collection type.
		/// </summary>
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		/// <summary>
		/// Gets <see cref="WebMethod"/> object using the index.
		/// </summary>
		/// <param name="index">
		/// The index.
		/// </param>
		/// <returns>
		/// The <see cref="WebMethod"/>.
		/// </returns>
		public User this[int index]
		{
			get
			{
				return this.BaseGet(index) as User;
			}
		}

		/// <summary>
		/// Gets <see cref="WebMethod"/> object using the key.
		/// </summary>
		/// <param name="key">
		/// The key.
		/// </param>
		/// <returns>
		/// The <see cref="WebMethod"/>.
		/// </returns>
		public new User this[string key]
		{
			get
			{
				return this.BaseGet(key) as User;
			}
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>
		/// The <see cref="IEnumerator"/>.
		/// </returns>
		public new IEnumerator<User> GetEnumerator()
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				yield return BaseGet(i) as User;
			}
		}
	}
}