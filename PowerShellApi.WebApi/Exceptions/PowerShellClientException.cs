namespace PowerShellRestApi.WebApi.Exceptions
{
	using System;
    using System.Management.Automation;

    /// <summary>
    /// The certificate not found exception.
    /// </summary>
    [Serializable]
	public class PowerShellClientException : Exception
	{
        /// <summary>
        /// Initialises a new instance of the <see cref="PowerShellClientException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public PowerShellClientException(string message, ErrorCategory category)
			: base(message)
		{
            this.Category = category;
		}
        /// <summary>
        /// Gets or sets the error category
        /// </summary>
        /// <value>
        /// The category of the error.
        /// </value>
        public ErrorCategory Category { get; set; }

    }
}