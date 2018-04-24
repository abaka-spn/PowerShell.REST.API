using PowerShellRestApi.Configuration;
using Microsoft.Owin.Security.ApiKey.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellRestApi.Owin
{
    public class Authentication
    {
        public static async Task ValidateIdentity(ApiKeyValidateIdentityContext context)
        {
            var user = WebApiConfiguration.Instance.Users.Where(u => u.ApiKey == context.ApiKey).FirstOrDefault();
            bool IpOk = false;

            if ( user != null)
            {
                if (user.IpAddresses.Count == 0)
                {
                    IpOk = true;
                }
                else
                {
                    IPAddress clientIPAddress = IPAddress.Parse(context.Request.RemoteIpAddress);

                    foreach (var userIP in user.IpAddresses)
                    {
                        IpOk = IPAddress.TryParse(userIP, out IPAddress userIPAddress) && userIPAddress.Equals(clientIPAddress);
                        if (IpOk)
                            break;
                    }
                }

                if (IpOk)
                    context.Validate();
            }
        }

        public static async Task<IEnumerable<Claim>> GenerateClaims(ApiKeyGenerateClaimsContext context)
        {
            return WebApiConfiguration.Instance.Users.Where(u => u.ApiKey == context.ApiKey).First().Identity.Claims;
        }

    }
}
