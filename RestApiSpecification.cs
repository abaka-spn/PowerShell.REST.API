using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using DynamicPowerShellApi.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace DynamicPowerShellApi
{
    public class RestApiSpecification
    {
        static OpenApiDocument _openApiDocument;

        public static OpenApiDocument CurrentDocument
        {
            get
            {
                if (_openApiDocument == null)
                    _openApiDocument = BuildSpecification();

                return _openApiDocument;
            }
        }

        public static string GetSpecAsV3()
        {
            var outputStringWriter = new StringWriter();
            var writer = new OpenApiJsonWriter(outputStringWriter);

            // Write V3 as JSON
            CurrentDocument.SerializeAsV3(writer);

            outputStringWriter.Flush();
            return outputStringWriter.GetStringBuilder().ToString();
        }

        public static string GetSpecAsV2()
        {
            var outputStringWriter = new StringWriter();
            var writer = new OpenApiJsonWriter(outputStringWriter);

            // Write V2 as JSON
            CurrentDocument.SerializeAsV2(writer);

            outputStringWriter.Flush();
            return outputStringWriter.GetStringBuilder().ToString();
        }

        public static OpenApiDocument BuildSpecification()
        {
            var openApiDocument = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = WebApiConfiguration.Instance.Version,
                    Title = WebApiConfiguration.Instance.Title,
                },
                Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = WebApiConfiguration.Instance.HostAddress.ToString() }
                    }
            };

            List<PSCommand> AppCommands = WebApiConfiguration.Instance
                .Apis.ToList()
                .SelectMany(x => x.WebMethods)
                .Select(x => x.ApiCommand)
                .ToList();

            if (AppCommands.Count > 0)
            {
                openApiDocument.Paths = new OpenApiPaths();

                foreach (var apiCmd in AppCommands)
                {
                    string routePath = apiCmd.GetRoutePath();

                    OperationType operationType = (OperationType)apiCmd.RestMethod;

                    if (openApiDocument.Paths.Where(x => x.Key == routePath).Count() == 0)
                        openApiDocument.Paths[routePath] = new OpenApiPathItem();

                    if (openApiDocument.Paths[routePath].Operations == null)
                    {
                        openApiDocument.Paths[routePath].Operations = new Dictionary<OperationType, OpenApiOperation>();
                    }

                    openApiDocument.Paths[routePath].Operations[operationType] = new OpenApiOperation
                    {
                        Description = apiCmd.Description,
                        OperationId = apiCmd.WebMethodName,
                        Summary = apiCmd.Synopsis,
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "OK",
                                Content = new Dictionary<string, OpenApiMediaType>()
                                        {
                                            {
                                                "application/json", new OpenApiMediaType()
                                            }
                                        }
                            }
                        },
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                                        {
                                            {apiCmd.GetRequestContentType(), new OpenApiMediaType{ } }
                                        }
                        }

                    };


                    // all Parameters, except by body
                    var openApiNotBodyParameters = new List<OpenApiParameter>();
                    foreach (var apiParameter in apiCmd.Parameters.Where(x => x.Location != RestLocation.Body))
                    {
                        openApiNotBodyParameters.Add
                        (
                            new OpenApiParameter
                            {
                                Name = apiParameter.Name,
                                Description = apiParameter.Description,
                                AllowEmptyValue = apiParameter.AllowEmpty,
                                Required = apiParameter.Required,
                                In = (ParameterLocation)apiParameter.Location,
                                Schema = apiParameter.GetSchemaOpenApiSchema()
                            }
                        );

                    }
                    if (openApiNotBodyParameters.Count > 0)
                        openApiDocument.Paths[routePath].Operations[operationType].Parameters = openApiNotBodyParameters;


                    // all body parameters
                    var openApiBodyProperties = new Dictionary<string, OpenApiSchema>();
                    bool required = false;
                    foreach (var apiParameter in apiCmd.Parameters.Where(x => x.Location == RestLocation.Body))
                    {
                        openApiBodyProperties.Add
                        (
                            apiParameter.Name,
                            apiParameter.GetSchemaOpenApiSchema()
                        );

                        required = required || apiParameter.Required;
                    }

                    if (openApiBodyProperties.Count > 0)
                        openApiDocument.Paths[routePath].Operations[operationType].RequestBody = new OpenApiRequestBody()
                        {
                            Required = required,
                            Content = new Dictionary<string, OpenApiMediaType>()
                                {
                                    {
                                        "application/json", new OpenApiMediaType()
                                        {
                                            Schema = new OpenApiSchema() { Type = "object",Properties = openApiBodyProperties}
                                        }
                                    }
                                }
                        };



                }
            }

            return openApiDocument;
        }

    }
}
