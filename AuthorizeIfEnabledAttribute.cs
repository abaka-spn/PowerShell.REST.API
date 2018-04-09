using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using DynamicPowerShellApi.Configuration;
using DynamicPowerShellApi.Exceptions;

namespace DynamicPowerShellApi
{
	/// <summary>	Attribute for authorize HTTP request if enabled. </summary>
	/// <remarks>	Anthony, 5/28/2015. </remarks>
	/// <seealso cref="T:System.Web.Http.AuthorizeAttribute"/>
	public class AuthorizeIfEnabledAttribute : AuthorizeAttribute
	{
		/// <summary>	Calls when an action is being authorized. </summary>
		/// <remarks>	Anthony, 5/28/2015. </remarks>
		/// <param name="actionContext">	The context. </param>
		/// <seealso cref="M:System.Web.Http.AuthorizeAttribute.OnAuthorization(HttpActionContext)"/>
		public override void OnAuthorization(HttpActionContext actionContext)
		{
            var request = actionContext.Request;

            DynamicPowershellApiEvents
            .Raise
            .ReceivedRequest(request.RequestUri.ToString());

            Guid activityId = Guid.NewGuid();

            if (request.RequestUri.Segments.Length < 4)
                throw new MalformedUriException(string.Format("There is {0} segments but must be at least 4 segments in the URI.", request.RequestUri.Segments.Length));

            // Check if Http Method is supported
            if (!Enum.TryParse(request.Method.Method, true, out RestMethod requestMethod))
            {
                // Check that the verbose messaging is working
                DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Http method ({0}) not supported", request.Method.Method));
                throw new WebApiNotFoundException(string.Format("Http method ({0}) not supported", request.Method.Method));
            }

            string route = request.RequestUri.Segments.Take(4).Aggregate((current, next) => current + next.ToLower());
            if (!WebApiConfiguration.Routes[requestMethod].ContainsKey(route))
            {
                // Check that the verbose messaging is working
                DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Cannot find the requested command for {0}", route));
                throw new WebApiNotFoundException(string.Format("Cannot find the requested command for {0}", route));
            }

            PSCommand psCommand = WebApiConfiguration.Routes[requestMethod][route];

            request.Properties["APP_PSCommand"] = psCommand;

            this.Roles = psCommand.Roles;
            this.Users = psCommand.Users;

            // Skip authentication if Anonymous allowed.
            if (psCommand.AllowAnonymous)
            {
                return;
            }
            else if (actionContext.RequestContext.Principal == null)
            {
                this.HandleUnauthorizedRequest(actionContext);
                return;
            }

            // Skip authentication if it's disabled in the config file.
            if (WebApiConfiguration.Instance.JwtAuthentication.Enabled == false && WebApiConfiguration.Instance.ApiKeyAuthentication.Enabled == false)
            {
                return;
            }
            else if (IsAuthorized(actionContext))
            {
                return;
            }
            else
            {
                this.HandleUnauthorizedRequest(actionContext);
			}
		}
	}
}
