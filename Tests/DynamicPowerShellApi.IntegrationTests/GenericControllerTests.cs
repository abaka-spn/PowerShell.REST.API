namespace PowerShellRestApi.IntegrationTests
{
	using System;
	using System.IdentityModel.Tokens;
	using System.Net;
	using System.Net.Http;
	using System.Security.Claims;

	using PowerShellRestApi.Owin;
	using PowerShellRestApi.Security;

	using Microsoft.Owin.Hosting;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text;

    /// <summary>
    /// The generic controller tests.
    /// </summary>
    [TestClass]
	public class GenericControllerTests
	{
		/// <summary>
		/// The issue token.
		/// </summary>
		/// <returns>
		/// The <see cref="string"/>.
		/// </returns>
		private static string IssueToken()
		{
            //string sec = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";
            string sec = Certificate.ReadCertificate().GetKeyAlgorithm();
            var now = DateTime.UtcNow;
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(sec));
            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                  securityKey,
                  SecurityAlgorithms.HmacSha256Signature);


            JwtSecurityToken jwt = new JwtSecurityToken(
				issuer: "Dummy Cloud STS",
				audience: "http://aperture.identity/connectors",
				claims: new[]
							{
								// TODO: Add your claims here.
								new Claim("sub", "someone@foo.bar.com")
							},
				notBefore: DateTime.UtcNow,
				expires: DateTime.UtcNow.AddHours(1),
                //TODO : change code to support new version
                //signingCredentials: new X509SigningCredentials(Certificate.ReadCertificate())
                signingCredentials: signingCredentials
            // signingCredentials: new HmacSigningCredentials(GetIssuerKey())
            );
			JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();

			return jwtHandler.WriteToken(jwt);
		}

		/// <summary>
		/// The process request run test script.
		/// </summary>
		[TestCategory("Integration Tests")]
		[TestMethod]
		public void ProcessRequestRunTestScript()
		{
			using (WebApp.Start<Startup>("http://localhost:9000"))
			{
				var client = new HttpClient { BaseAddress = new Uri("http://localhost:9000") };
				client.SetBearerToken(IssueToken());
				var response = client.GetAsync("/api/Exchange/CreateMailbox?MailBoxSize=99").Result;

				Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Could not authenticate!");
				Assert.IsTrue(response.IsSuccessStatusCode, "Response was not successful.");
			}
		}
	}
}
