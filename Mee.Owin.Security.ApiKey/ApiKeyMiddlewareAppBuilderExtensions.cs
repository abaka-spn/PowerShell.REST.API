using Owin;

namespace Mee.Owin.Security.ApiKey
{
    public static class ApiKeyMiddlewareAppBuilderExtensions
    {

        public static void UseApiKeyMiddleware(this IAppBuilder app)
        {
            app.Use<ApiKeyMiddleware>();
        }

    }

}