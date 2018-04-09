using DynamicPowerShellApi.Jobs;
using DynamicPowerShellApi.Logging;

namespace DynamicPowerShellApi.Owin
{
	using Autofac;
	using Autofac.Integration.WebApi;

	using Configuration;
	using Controllers;
	using DynamicPowerShellApi;

	using Microsoft.Owin.Hosting;
	using Microsoft.Owin.Security;
	using Microsoft.Owin.Security.Jwt;
	using global::Owin;

	using Security;
	using System;
	using System.Security.Cryptography.X509Certificates;
	using System.Web.Http;
	using System.Web.Http.Controllers;
	using System.Web.Http.Dispatcher;
    using Microsoft.Owin.StaticFiles;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin;
    using Microsoft.Owin.Security.ApiKey;
    using System.Threading.Tasks;
    using System.Security.Claims;
    using System.Collections.Generic;
    using Microsoft.Owin.Security.ApiKey.Contexts;

    /// <summary>
    /// The startup.
    /// </summary>
    public class Startup
	{
		/// <summary>
		/// This code configures Web API. The Startup class is specified as a type
		/// parameter in the WebApp.Start method.
		/// </summary>
		/// <param name="appBuilder">The application builder.</param>
		public void Configuration(IAppBuilder appBuilder)
		{
			// Configure Web API for self-host. 
			HttpConfiguration config = CreateConfiguration();

			// Construct the Autofac container
			IContainer container = BuildContainer();

			// Use autofac's dependency resolver, not the OWIN one
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			// Wait for the initialization to complete (setup the socket)
			config.EnsureInitialized();

			// If the config file specifies authentication, load up the certificates and use the JWT middleware.
		    if (WebApiConfiguration.Instance.JwtAuthentication.Enabled)
		    {
		        X509Certificate2 cert = Certificate.ReadCertificate();

		        appBuilder.UseJwtBearerAuthentication(
		            new JwtBearerAuthenticationOptions
		            {
		                AllowedAudiences = new[]
		                {
			                WebApiConfiguration.Instance.JwtAuthentication.Audience
		                },
		                IssuerSecurityTokenProviders =
		                    new[]
		                    {
		                        new X509CertificateSecurityTokenProvider(cert.Subject, cert)
		                    },
		                AuthenticationType = "Bearer",
		                AuthenticationMode = AuthenticationMode.Active
		            });
		    }

            // If the config file specifies authentication, load up ApiKey middleware.
            if (WebApiConfiguration.Instance.ApiKeyAuthentication.Enabled)
            {
                appBuilder.UseApiKeyAuthentication(new ApiKeyAuthenticationOptions()
                {
                    Header = WebApiConfiguration.Instance.ApiKeyAuthentication.Header ?? "Authorization",
                    HeaderKey = WebApiConfiguration.Instance.ApiKeyAuthentication.HeaderKey ?? "ApiKey",
                    Provider = new ApiKeyAuthenticationProvider()
                    {
                        OnValidateIdentity = Authentication.ValidateIdentity,
                        OnGenerateClaims = Authentication.GenerateClaims
                    }
                });
            }

            var options = new FileServerOptions
            {
                RequestPath = new PathString("/help"),
                EnableDirectoryBrowsing = false,
                EnableDefaultFiles = true,
                DefaultFilesOptions = { DefaultFileNames = { "index.html" } },
                FileSystem = new PhysicalFileSystem("Swagger-UI")
            };


            appBuilder.UseAutofacMiddleware(container);

            appBuilder.UseWebApi(config);
/*
 * TO DO : Change the content of index.html to customize the Openapi specification URL 
            appBuilder.Map("/help", spa =>
            {
                spa.Use((context, next) =>
                {
                    context.Request.Path = new PathString("/Swagger-UI/index.html");
                    return next();
                });
                spa.UseStaticFiles();
            });
*/ 
            appBuilder.UseFileServer(options);
            appBuilder.UseAutofacWebApi(config);
		}

		/// <summary>
		/// The build container.
		/// </summary>
		/// <returns>
		/// The <see cref="IContainer"/>.
		/// </returns>
		private IContainer BuildContainer()
		{
			ContainerBuilder builder = new ContainerBuilder();

			builder.RegisterApiControllers(typeof(GenericController).Assembly);

			builder
				.RegisterType<JobListProvider>()
				.SingleInstance()
				.As<IJobListProvider>();

			builder.RegisterType<PowershellRunner>()
				.As<IRunner>()
				.InstancePerDependency();

			builder.RegisterType<CrashLogger>()
				.As<ICrashLogger>()
				.SingleInstance();

			return builder.Build();
		}

		/// <summary>
		/// The create configuration.
		/// </summary>
		/// <returns>
		/// The <see cref="HttpConfiguration"/>.
		/// </returns>
		private HttpConfiguration CreateConfiguration()
		{
			// Configure Web API for self-host. 
			HttpConfiguration config = new HttpConfiguration();

			config.Services.Replace(typeof(IHttpControllerSelector), new GenericControllerSelector(config));
			config.Services.Replace(typeof(IHttpActionSelector), new GenericActionSelector(config));

            config.MapHttpAttributeRoutes();

            /*
            config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{action}/{id1}/{id2}/{id3}",
				defaults: new { category = "all",
                                id1 = RouteParameter.Optional,
                                id2 = RouteParameter.Optional,
                                id3 = RouteParameter.Optional
                });
                */
            /*
            config.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            config.Routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
*/

            return config;
		}

		/// <summary>
		/// The start.
		/// </summary>
		/// <returns>
		/// The <see cref="IDisposable"/>.
		/// </returns>
		public static IDisposable Start()
		{

            WebApiConfiguration.LoadPSCommands();

            //var options = new StartOptions();
            //options.Urls.Add(baseAddress.ToString());


            Uri baseAddress = WebApiConfiguration.Instance.HostAddress;

			// Start OWIN host 
			return WebApp.Start<Startup>(url: baseAddress.ToString());
		}

        public static string BaseUrl => WebApiConfiguration.Instance.HostAddress.ToString();
    }
}