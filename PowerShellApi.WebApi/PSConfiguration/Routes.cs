using PowerShellRestApi.PSConfiguration;
using PowerShellRestApi.WebApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellRestApi.PSConfiguration
{
    public sealed class Routes
    {
        /// <summary>
        /// The singleton object of the Routes for all PSCOmmand.
        /// </summary>
        private static readonly Lazy<Routes> LazyRoutes =
            new Lazy<Routes>(() => new Routes());

        public static Dictionary<RestMethod, Dictionary<string, PSCommand>> Instance { get { return LazyRoutes.Value._routes; } }


        Dictionary<RestMethod, Dictionary<string, PSCommand>> _routes;

        private Routes()
        {
            _routes = new Dictionary<RestMethod, Dictionary<string, PSCommand>>
            {
                { RestMethod.Get, new Dictionary<string, PSCommand>() },
                { RestMethod.Post, new Dictionary<string, PSCommand>() },
                { RestMethod.Put, new Dictionary<string, PSCommand>() },
                { RestMethod.Patch, new Dictionary<string, PSCommand>() },
                { RestMethod.Delete, new Dictionary<string, PSCommand>() }
            };
        }

        public static void Add(PSCommand Command)
        {
            string route = (new Uri("http://localhost" + Command.GetRoutePath())).Segments.Take(4).Aggregate((current, next) => current + next.ToLower());

            Routes.Instance[Command.RestMethod][route] = Command;
        }

        public static PSCommand Get(Uri RequestUri, string HttpMethod)
        {
            if (RequestUri.Segments.Length < 4)
                throw new MalformedUriException(string.Format("There is {0} segments but must be at least 4 segments in the URI.", RequestUri.Segments.Length));

            // Check if Http Method is supported
            if (!Enum.TryParse(HttpMethod, true, out RestMethod requestMethod))
            {
                // Check that the verbose messaging is working
                PowerShellRestApiEvents.Raise.VerboseMessaging(String.Format("Http method ({0}) not supported", HttpMethod));
                throw new WebApiNotFoundException(string.Format("Http method ({0}) not supported", HttpMethod));
            }

            string route = RequestUri.Segments.Take(4).Aggregate((current, next) => current + next.ToLower());

            if (!Routes.Instance[requestMethod].ContainsKey(route))
            {
                // Check that the verbose messaging is working
                PowerShellRestApiEvents.Raise.VerboseMessaging(String.Format("Cannot find the requested command for {0}", route));
                throw new WebApiNotFoundException(string.Format("Cannot find the requested command for {0}", route));
            }

            return LazyRoutes.Value._routes[requestMethod][route];
        }

        public static List<PSCommand> GetAll()
        {
            return LazyRoutes.Value._routes.SelectMany(x => x.Value).Select(x => x.Value).ToList();
        }
    }
}
