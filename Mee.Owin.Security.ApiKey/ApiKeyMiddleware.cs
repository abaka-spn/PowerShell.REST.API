using Microsoft.Owin;
using System.Linq;
using System.Threading.Tasks;

namespace Mee.Owin.Security.ApiKey
{
    public class ApiKeyMiddleware : OwinMiddleware
    {

        public ApiKeyMiddleware(OwinMiddleware next) : base(next)
        {
            Manager.GetInstance().Load();
        }

        public async override Task Invoke(IOwinContext context)
        {

            string[] apiKeyHeaderValues = null;
            if (context.Request.Headers.TryGetValue("X-ApiKey", out apiKeyHeaderValues))
            {
                var apiKeyHeaderValue = apiKeyHeaderValues.First();
                User apiKeyUser = Manager.GetInstance().GetUser(apiKeyHeaderValue);

                if(apiKeyUser != null)
                {
                    context.Request.User = apiKeyUser.GetClaimsPrincipal();
                }

            }

            await Next.Invoke(context);
        }
    }
}