namespace DynamicPowerShellApi.Exceptions
{
	using System;

	/// <summary>
	/// The certificate not found exception.
	/// </summary>
	[Serializable]
	public class PScommandNotFoundException : Exception
	{
        /// <summary>
        /// Initialises a new instance of the <see cref="PScommandNotFoundException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public PScommandNotFoundException(string message)
			: base(message)
		{
		}
	}
}