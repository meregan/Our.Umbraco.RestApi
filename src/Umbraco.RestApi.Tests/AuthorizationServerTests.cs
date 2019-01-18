using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Security;
using Umbraco.RestApi.Tests.TestHelpers;
using Umbraco.Web.Security.Identity;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.RestApi.Tests
{
    [TestFixture]
    public class AuthorizationServerTests
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.Current.ToString(3));
        }

        [TearDown]
        public void TearDown()
        {
            //Hack - because Reset is internal
            typeof(PropertyEditorResolver).CallStaticMethod("Reset", true);
            UmbracoRestApiOptionsInstance.Options = new UmbracoRestApiOptions();
        }

        [Test]
        public async Task Get_Token()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {

                });

            var authServerOptions = new UmbracoAuthorizationServerProviderOptions
            {
                Secret = "abcdefghijklmnopqrstuvwxyz12345678909876543210",
                Audience = "test",
                AllowInsecureHttp = true
            };
            using (var server = TestServer.Create(app =>
            {
                ConfigureUserManager(app, new ClaimsIdentity(new[] {new Claim("test", "test")}), startup.ApplicationContext);
                app.UseUmbracoTokenAuthentication(authServerOptions);
                var httpConfig = startup.UseTestWebApiConfiguration(app);
                app.UseUmbracoRestApi(startup.ApplicationContext, new UmbracoRestApiOptions
                {
                    //customize the authz policies, in this case we want to allow everything
                    CustomAuthorizationPolicyCallback = (policyName, defaultPolicy) => (builder => builder.RequireAssertion(context => true))
                });
                app.UseWebApi(httpConfig);
            }))
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"http://testserver{authServerOptions.AuthEndpoint}"),
                    Method = HttpMethod.Post,
                    Content = new StringContent(
                        "grant_type=password&username=YOURUSERNAME&password=YOURPASSWORD", 
                        Encoding.UTF8, 
                        "application/x-www-form-urlencoded")
                };
                
                //add the origin so Cors kicks in!
                request.Headers.Add("Origin", "http://localhost:12061");
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                var djson = JsonConvert.DeserializeObject<JObject>(json);
                Console.Write(JsonConvert.SerializeObject(djson, Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                Assert.IsTrue(result.Headers.Contains("Access-Control-Allow-Origin"));
                var acao = result.Headers.GetValues("Access-Control-Allow-Origin");
                Assert.AreEqual(1, acao.Count());

                //looks like the mvc cors default is to allow the request domain instea of *
                Assert.AreEqual("http://localhost:12061", acao.First());

                Assert.IsNotNull(djson["access_token"].Value<string>());
                Assert.AreEqual("bearer", djson["token_type"].Value<string>());
                Assert.IsNotNull(djson["expires_in"].Value<string>());
            }
        }

        [Test]
        public async Task Use_Token()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {

                });

            var authServerOptions = new UmbracoAuthorizationServerProviderOptions
            {
                Secret = "abcdefghijklmnopqrstuvwxyz12345678909876543210",
                Audience = "test",
                AllowInsecureHttp = true
            };
            using (var server = TestServer.Create(app =>
            {
                ConfigureUserManager(app,
                    //For published content we can have a normal ClaimsIdentity
                    new ClaimsIdentity(new[]
                    {
                        new Claim(AuthorizationPolicies.UmbracoRestApiClaimType, "true", ClaimValueTypes.Boolean, AuthorizationPolicies.UmbracoRestApiIssuer),

                        new Claim(ClaimTypes.Locality, "en-US", ClaimValueTypes.String, AuthorizationPolicies.UmbracoRestApiIssuer),
                        new Claim(ClaimTypes.GivenName, "Admin", ClaimValueTypes.String, AuthorizationPolicies.UmbracoRestApiIssuer),
                        new Claim(ClaimTypes.Name, "admin@umbraco.com", ClaimValueTypes.String, AuthorizationPolicies.UmbracoRestApiIssuer),
                        new Claim(ClaimTypes.NameIdentifier, "0", ClaimValueTypes.Integer, AuthorizationPolicies.UmbracoRestApiIssuer),                                                
                        new Claim(Constants.Security.AllowedApplicationsClaimType, "content", ClaimValueTypes.String, AuthorizationPolicies.UmbracoRestApiIssuer),
                        new Claim(Constants.Security.StartContentNodeIdClaimType, "[-1]", ClaimValueTypes.String, AuthorizationPolicies.UmbracoRestApiIssuer),
                        new Claim(Constants.Security.StartMediaNodeIdClaimType, "[-1]", ClaimValueTypes.String, AuthorizationPolicies.UmbracoRestApiIssuer),

                    }), startup.ApplicationContext);

                app.UseUmbracoTokenAuthentication(authServerOptions);
                var httpConfig = startup.UseTestWebApiConfiguration(app);
                app.UseUmbracoRestApi(startup.ApplicationContext, new UmbracoRestApiOptions
                {
                    //customize the authz policies, in this case we want to allow everything
                    CustomAuthorizationPolicyCallback = (policyName, defaultPolicy) => (builder => builder.RequireAssertion(context => true))
                });
                app.UseWebApi(httpConfig);
            }))
            {
                var tokenRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri($"http://testserver{authServerOptions.AuthEndpoint}"),
                    Method = HttpMethod.Post,
                    Content = new StringContent(
                        "grant_type=password&username=YOURUSERNAME&password=YOURPASSWORD",
                        Encoding.UTF8,
                        "application/x-www-form-urlencoded")
                };

                //add the origin so Cors kicks in!
                tokenRequest.Headers.Add("Origin", "http://localhost:12061");
                Console.WriteLine(tokenRequest);
                var result = await server.HttpClient.SendAsync(tokenRequest);
                Console.WriteLine(result);
                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                var djson = JsonConvert.DeserializeObject<JObject>(json);
                Console.Write(JsonConvert.SerializeObject(djson, Formatting.Indented));
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
                var token = djson["access_token"].Value<string>();


                //Now we can use the token

                var accessRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{RouteConstants.ContentSegment}"),
                    Method = HttpMethod.Get,
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                };

                accessRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                accessRequest.Headers.Add("Access-Control-Request-Headers", "accept, authorization");
                accessRequest.Headers.Add("Access-Control-Request-Method", "GET");
                accessRequest.Headers.Add("Origin", "http://localhost:12061");
                accessRequest.Headers.Add("Referer", "http://localhost:12061/browser.html");

                Console.WriteLine(accessRequest);
                result = await server.HttpClient.SendAsync(accessRequest);
                Console.WriteLine(result);

                json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            }
        }


        private void ConfigureUserManager(IAppBuilder app, ClaimsIdentity identity, ApplicationContext appCtx)
        {
            var claimsFactory = new Mock<IClaimsIdentityFactory<BackOfficeIdentityUser, int>>();
            claimsFactory.Setup(x => x.CreateAsync(It.IsAny<UserManager<BackOfficeIdentityUser, int>>(), It.IsAny<BackOfficeIdentityUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(identity));
            var userStore = Mock.Of<IUserStore<BackOfficeIdentityUser, int>>();
            var backOfficeUserManager = new Mock<BackOfficeUserManager>(userStore);
            backOfficeUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new BackOfficeIdentityUser(1, new IReadOnlyUserGroup[] { })));
            backOfficeUserManager.Object.ClaimsIdentityFactory = claimsFactory.Object;

            app.ConfigureUserManagerForUmbracoBackOffice<BackOfficeUserManager, BackOfficeIdentityUser>(
                appCtx,
                (options, context) => backOfficeUserManager.Object);
        }
    }
}