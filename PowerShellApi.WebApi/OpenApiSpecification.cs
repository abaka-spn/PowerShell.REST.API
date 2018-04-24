using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using PowerShellRestApi.Configuration;
using PowerShellRestApi.PSConfiguration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace PowerShellRestApi.WebApi
{
    public class OpenApiSpecification
    {
        static OpenApiDocument _openApiDocument;

        public static OpenApiDocument CurrentDocument
        {
            get
            {
                //if (_openApiDocument == null)
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

            /* Sample
            openApiDocument.Components = new OpenApiComponents
            {
                Schemas =
                        {
                            ["ErrorModel"] = new OpenApiSchema
                            {
                                Type = "object",
                                Properties =
                                {
                                    ["code"] = new OpenApiSchema
                                    {
                                        Type = "integer",
                                        Minimum = 100,
                                        Maximum = 600
                                    },
                                    ["message"] = new OpenApiSchema
                                    {
                                        Type = "string"
                                    }
                                },
                                Required =
                                {
                                    "message",
                                    "code"
                                }
                            },
                            ["ExtendedErrorModel"] = new OpenApiSchema
                            {
                                AllOf =
                                {
                                    new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = "ErrorModel"
                                        },
                                        // Schema should be dereferenced in our model, so all the properties
                                        // from the ErrorModel above should be propagated here.
                                        Type = "object",
                                        Properties =
                                        {
                                            ["code"] = new OpenApiSchema
                                            {
                                                Type = "integer",
                                                Minimum = 100,
                                                Maximum = 600
                                            },
                                            ["message"] = new OpenApiSchema
                                            {
                                                Type = "string"
                                            }
                                        },
                                        Required =
                                        {
                                            "message",
                                            "code"
                                        }
                                    },
                                    new OpenApiSchema
                                    {
                                        Type = "object",
                                        Required = {"rootCause"},
                                        Properties =
                                        {
                                            ["rootCause"] = new OpenApiSchema
                                            {
                                                Type = "string"
                                            }
                                        }
                                    }
                                }
                            }
                        }
            };
            */

            if (PSModel.AllModels.Count > 0)
            {
                //var openApiComponents = new Dictionary<string, OpenApiComponents>();

                var openApiComponents = new OpenApiComponents();

                foreach (var psModel in PSModel.AllModels.Values)
                {
                    openApiComponents.Schemas.Add(psModel.Name, psModel.GetOpenApiSchema());
                }

                openApiDocument.Components = openApiComponents;
            }

            /*
                        List<PSCommand> AppCommands = WebApiConfiguration.Instance
                            .Apis.ToList()
                            .SelectMany(x => x.WebMethods)
                            .Select(x => x.GetApiCommand())
                            .ToList();
                            */
            List<PSCommand> AppCommands = Routes.GetAll().OrderBy(x => x.ApiName).ToList();

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


                    // all Parameters, except body
                    var openApiNotBodyParameters = new List<OpenApiParameter>();
                    foreach (var apiParameter in apiCmd.GetParametersExceptForLocation(RestLocation.Body))
                    {
                        openApiNotBodyParameters.Add
                        (
                            apiParameter.GetOpenApiParameter()
                        );
                    }
                    if (openApiNotBodyParameters.Count > 0)
                        openApiDocument.Paths[routePath].Operations[operationType].Parameters = openApiNotBodyParameters;


                    // all body parameters
                    var openApiBodySchema = apiCmd.GetOpenApiSchema();
                    if (openApiBodySchema.Properties.Count > 0)
                    {
                        openApiDocument.Paths[routePath].Operations[operationType].RequestBody = new OpenApiRequestBody()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                                {
                                    {
                                        "application/json",
                                        new OpenApiMediaType()
                                        {
                                            Schema = openApiBodySchema
                                        }
                                    }
                                }
                        };
                    }
                }
            }

            return openApiDocument;
        }

    }
}
