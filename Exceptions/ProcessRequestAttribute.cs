using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

namespace DynamicPowerShellApi.Exceptions
{
    public class ProcessRequestAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is MissingParametersException ||
                context.Exception is MalformedUriException ||
                context.Exception is MissingParametersException ||
                context.Exception is WebApiNotFoundException ||
               context.Exception is WebMethodNotFoundException)
            {
                context.Response =  context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, context.Exception.Message);
            }
        }
    }
}
