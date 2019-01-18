using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OAuth.Messages;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// A simple OAuth server provider to verify back office users
    /// </summary>
    public class UmbracoAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private readonly UmbracoAuthorizationServerProviderOptions _options;

        public UmbracoAuthorizationServerProvider(UmbracoAuthorizationServerProviderOptions options = null)
        {
            if (options == null)
                options = new UmbracoAuthorizationServerProviderOptions();
            _options = options;
        }

        /// <summary>
        /// Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are
        /// present on the request. If the web application accepts Basic authentication credentials,
        /// context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. If the web
        /// application accepts "client_id" and "client_secret" as form encoded POST parameters,
        /// context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body.
        /// If context.Validated is not called the request will not proceed further.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //TODO: This would work but would it be better to just have two different endpoints setup - one for members, one for users.
            //      Each of those would then support the "password", "authorization_code" grant_type

            //check for a custom parameter in the request which will dictate how with authenticate
            var uid = context.Parameters.Where(f => f.Key == "umb_auth").Select(f => f.Value).SingleOrDefault();
            if (uid == null || uid.Length == 0)
            {
                uid = new[] { UmbracoAuthType.UserPassword.ToString() };
            }
            context.OwinContext.Set("umb:authtype", uid[0]);

            var tokenEndpointRequest = new TokenEndpointRequest(context.Parameters);
            if (tokenEndpointRequest.IsAuthorizationCodeGrantType)
            {
                if (!context.TryGetBasicCredentials(out var clientId, out _))
                {
                    context.TryGetFormCredentials(out clientId, out _);
                }

                if (context.ClientId == null)
                {
                    context.Rejected();
                    context.SetError("invalid_client", "Client credentials could not be retrieved through the Authorization header.");
                    return Task.FromResult(0);
                }

                context.Validated(clientId);
            }
            else
            {
                context.Validated();
            }
            
            return Task.FromResult(0);

            // Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are
            // present on the request. If the web application accepts Basic authentication credentials, 
            // context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. If the web 
            // application accepts "client_id" and "client_secret" as form encoded POST parameters, 
            // context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body.
            // If context.Validated is not called the request will not proceed further. 

            //** Currently we just accept everything globally
            //context.Validated();
            //return Task.FromResult(0);

            // Example for checking registered clients:

            //** Validate that the data is in the request
            //string clientId;
            //string clientSecret;
            //if (context.TryGetFormCredentials(out clientId, out clientSecret) == false)
            //{
            //    context.SetError("invalid_client", "Form credentials could not be retrieved.");
            //    context.Rejected();
            //    return Task.FromResult(0);
            //}

            //var userManager = context.OwinContext.GetUserManager<BackOfficeUserManager>();

            //** Check if this client id is allowed/registered
            // - lookup in custom table

            //** Verify that the client id and client secret match 
            //if (client != null && userManager.PasswordHasher.VerifyHashedPassword(client.ClientSecretHash, clientSecret) == PasswordVerificationResult.Success)
            //{
            //    // Client has been verified.
            //    context.Validated(clientId);
            //}
            //else
            //{
            //    // Client could not be validated.
            //    context.SetError("invalid_client", "Client credentials are invalid.");
            //    context.Rejected();
            //}
        }

        /// <summary>
        /// Called at the final stage of a successful Token endpoint request. An application may implement this call in order to do any final 
        ///             modification of the claims being used to issue access or refresh tokens. This call may also be used in order to add additional 
        ///             response parameters to the Token endpoint's json response body.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        /// <remarks>
        /// This validates the grant_type accepted and also processes CORS
        /// </remarks>
        public override Task ValidateTokenRequest(OAuthValidateTokenRequestContext context)
        {   
            //TODO: Determine which grant types will will actually support - these will probably be the only ones
            if (!context.TokenRequest.IsAuthorizationCodeGrantType &&
                !context.TokenRequest.IsResourceOwnerPasswordCredentialsGrantType &&
                !context.TokenRequest.IsRefreshTokenGrantType)
            {
                context.Rejected();
                context.SetError("invalid_grant_type", "Only grant_type=authorization_code, grant_type=password or grant_type=refresh_token are accepted by this server.");
                return Task.FromResult(0);
            }

            ProcessCors(context);

            return base.ValidateTokenRequest(context);
        }

        /// <summary>
        /// Based on the information in the context will assign a ClaimsIdentity
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        /// NOTE: This is "Resource Owner Password Credential Flow"
        /// </remarks>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //TODO: Based on the context we need to create a ClaimsIdentity

            var authType = context.OwinContext.Get<string>("umb:authtype");
            if (!Enum<UmbracoAuthType>.TryParse(authType, true, out UmbracoAuthType parsed))
            {
                context.Rejected();
                context.SetError("invalid_umb_auth", "Unsupported Umbraco umb_auth");
                return;
            }

            switch (parsed)
            {
                case UmbracoAuthType.UserPassword:

                    var userManager = context.OwinContext.GetUserManager<BackOfficeUserManager>();
                    if (userManager == null)
                        throw new InvalidOperationException("No " + typeof(BackOfficeUserManager) + " found in the owin context");

                    var user = await userManager.FindAsync(context.UserName, context.Password);

                    if (user == null)
                    {
                        context.SetError("invalid_grant", "The user name or password is incorrect.");
                        return;
                    }

                    //TODO: This way Or the one below? same thing?
                    //var identity = user.GenerateUserIdentityAsync(userManager);
                    var identity = await userManager.ClaimsIdentityFactory.CreateAsync(userManager, user, context.Options.AuthenticationType);

                    context.Validated(identity);

                    break;
                case UmbracoAuthType.MemberPassword:
                    break;
                case UmbracoAuthType.UserAuthCode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private void ProcessCors(OAuthValidateTokenRequestContext context)
        {
            var accessControlRequestMethodHeaders = context.Request.Headers.GetCommaSeparatedValues(CorsConstants.AccessControlRequestMethod);
            var originHeaders = context.Request.Headers.GetCommaSeparatedValues(CorsConstants.Origin);
            var accessControlRequestHeaders = context.Request.Headers.GetCommaSeparatedValues(CorsConstants.AccessControlRequestMethod);
            var corsRequest = new CorsRequestContext
            {
                Host = context.Request.Host.Value,
                HttpMethod = context.Request.Method,
                Origin = originHeaders?.FirstOrDefault(),
                RequestUri = context.Request.Uri,
                AccessControlRequestMethod = accessControlRequestMethodHeaders?.FirstOrDefault()
            };
            if (accessControlRequestHeaders != null)
            {
                foreach (var header in context.Request.Headers.GetCommaSeparatedValues(CorsConstants.AccessControlRequestMethod))
                {
                    corsRequest.AccessControlRequestHeaders.Add(header);
                }
            }

            var engine = new CorsEngine();

            if (corsRequest.IsPreflight)
            {
                try
                {
                    // Make sure Access-Control-Request-Method is valid.
                    var test = new HttpMethod(corsRequest.AccessControlRequestMethod);
                }
                catch (ArgumentException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.SetError("Access Control Request Method Cannot Be Null Or Empty");
                    //context.RequestCompleted();
                    return;
                }
                catch (FormatException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.SetError("Invalid Access Control Request Method");
                    //context.RequestCompleted();
                    return;
                }

                var result = engine.EvaluatePolicy(corsRequest, _options.CorsPolicy);

                if (!result.IsValid)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.SetError(string.Join(" | ", result.ErrorMessages));
                    //context.RequestCompleted();
                    return;
                }

                WriteCorsHeaders(result, context);
            }
            else
            {
                var result = engine.EvaluatePolicy(corsRequest, _options.CorsPolicy);

                if (result.IsValid)
                {
                    WriteCorsHeaders(result, context);
                }
            }
        }

        private void WriteCorsHeaders(CorsResult result, OAuthValidateTokenRequestContext context)
        {
            var headers = result.ToResponseHeaders();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    context.Response.Headers.Append(header.Key, header.Value);
                }
            }
        }
    }
}