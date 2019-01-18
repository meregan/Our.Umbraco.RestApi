using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Authorization;
using Microsoft.Owin.Security.Authorization.Infrastructure;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.RestApi.Security;
using Umbraco.Web.Security.Identity;

namespace Umbraco.RestApi
{
    public static class AppBuilderExtensions
    {

        //TODO: Need to follow more of this for http://bitoftech.net/2014/10/27/json-web-token-asp-net-web-api-2-jwt-owin-authorization-server/ dealing with AudienceStores

        //another interesting one https://blog.jayway.com/2014/09/25/securing-asp-net-web-api-endpoints-using-owin-oauth-2-0-and-claims/

        /// <summary>
        /// Configures Umbraco to issue and process authentication tokens
        /// </summary>
        /// <param name="app"></param>
        /// <param name="authServerProviderOptions"></param>
        /// <remarks>
        /// This is a very simple implementation of token authentication, the expiry below is for a single day and with
        /// this implementation there is no way to force expire tokens on the server however given the code below and the additional
        /// callbacks that can be registered for the BackOfficeAuthServerProvider these types of things could be implemented. Additionally the
        /// BackOfficeAuthServerProvider could be overridden to include this functionality instead of coding the logic into the callbacks.
        /// </remarks>
        /// <example>
        /// 
        /// An example of using this implementation is to use the UmbracoStandardOwinSetup and execute this extension method as follows:
        /// 
        /// <![CDATA[
        /// 
        ///   public override void Configuration(IAppBuilder app)
        ///   {
        ///       //ensure the default options are configured
        ///       base.Configuration(app);
        ///   
        ///       //configure token auth
        ///       app.UseUmbracoBackOfficeTokenAuth();
        ///   }
        /// 
        /// ]]>
        /// 
        /// Then be sure to read the details in UmbracoStandardOwinSetup on how to configure Owin to startup using it.
        /// </example>
        public static void UseUmbracoTokenAuthentication(this IAppBuilder app, UmbracoAuthorizationServerProviderOptions authServerProviderOptions = null)
        {
            authServerProviderOptions = authServerProviderOptions ?? new UmbracoAuthorizationServerProviderOptions();

            //if a secret is supplied then 
            var base64Key = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    authServerProviderOptions.Secret));

            var tokenProvider = new SymmetricKeyIssuerSecurityTokenProvider(
                AuthorizationPolicies.UmbracoRestApiIssuer,
                base64Key);

            var oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                //generally you wouldn't allow this unless on SSL!
                AllowInsecureHttp = authServerProviderOptions.AllowInsecureHttp,
                TokenEndpointPath = new PathString(authServerProviderOptions.AuthEndpoint),
                AuthenticationType = AuthorizationPolicies.UmbracoRestApiTokenAuthenticationType,
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new UmbracoAuthorizationServerProvider(authServerProviderOptions)
            };

            oAuthServerOptions.AccessTokenFormat = new JwtFormatWriter(
                oAuthServerOptions,
                tokenProvider.Issuer,
                authServerProviderOptions.Audience,
                base64Key);

            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AllowedAudiences = new[] { authServerProviderOptions.Audience },
                IssuerSecurityTokenProviders = new[] { tokenProvider },
                
                Provider = new OAuthBearerAuthenticationProvider
                {
                    OnApplyChallenge = context => { return Task.FromResult(0); },
                    OnRequestToken = context => { return Task.FromResult(0); },
                    OnValidateIdentity = context =>
                    {
                        //ensure that the rest api claim is added to the ticket if everything is validated
                        if (context.IsValidated)
                        {
                            context.Ticket.Identity.AddClaim(new Claim(AuthorizationPolicies.UmbracoRestApiClaimType, "true", ClaimValueTypes.Boolean, AuthorizationPolicies.UmbracoRestApiIssuer));
                        }
                        return Task.FromResult(0);
                    }
                }
            });
        }

        /// <summary>
        /// Required call to enable the REST API
        /// </summary>
        /// <param name="app"></param>
        /// <param name="applicationContext"></param>
        /// <param name="options">
        /// Options to configure the rest api including CORS and Authorization policies
        /// </param>
        public static void UseUmbracoRestApi(this IAppBuilder app,
            ApplicationContext applicationContext,
            UmbracoRestApiOptions options = null)
        {
            if (applicationContext == null) throw new ArgumentNullException(nameof(applicationContext));
            UmbracoRestApiOptionsInstance.Options = options ?? new UmbracoRestApiOptions();

            app.UseUmbracoRestApiAuthorizationPolicies(applicationContext, UmbracoRestApiOptionsInstance.Options.CustomAuthorizationPolicyCallback);
        }

        /// <summary>
        /// Authorization for the rest API uses authorization schemes, see https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies
        /// </summary>
        /// <param name="app"></param>
        /// <param name="applicationContext"></param>
        /// <param name="customPolicyCallback"></param>
        /// <remarks>
        /// This is a .NET Core approach to authorization but we have a package that has backported all of this for us for .NET 452
        /// </remarks>
        internal static void UseUmbracoRestApiAuthorizationPolicies(
            this IAppBuilder app,
            ApplicationContext applicationContext,
            Func<string, AuthorizationPolicy, Action<AuthorizationPolicyBuilder>> customPolicyCallback = null)
        {
            app.UseAuthorization(options =>
            {
                //A policy that is used at a top level controller - this is more or less to ensure that any actions that 
                //don't have an explicit authz policy assigned will defer to this default one

                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.DefaultRestApi,
                    policy => policy.RequireSessionIdOrRestApiClaim(),
                    customPolicyCallback);

                //Published Content READ

                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.PublishedContentRead,
                    policy => policy.RequireSessionIdOrRestApiClaim(),
                    customPolicyCallback);

                //Content CRUD

                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.ContentRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionBrowse.Instance.Letter.ToString()));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.ContentCreate,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionNew.Instance.Letter.ToString(), ActionUpdate.Instance.Letter.ToString()));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.ContentUpdate,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionUpdate.Instance.Letter.ToString(), ActionPublish.Instance.Letter.ToString()));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.ContentDelete,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionDelete.Instance.Letter.ToString()));
                    },
                    customPolicyCallback);

                //Media CRUD

                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MediaRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Media));
                        policy.Requirements.Add(new MediaPermissionRequirement());
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MediaCreate,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Media));
                        policy.Requirements.Add(new MediaPermissionRequirement());
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MediaUpdate,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Media));
                        policy.Requirements.Add(new MediaPermissionRequirement());
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MediaDelete,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Media));
                        policy.Requirements.Add(new MediaPermissionRequirement());
                    },
                    customPolicyCallback);

                //Members CRUD

                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MemberRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Members));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MemberCreate,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Members));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MemberUpdate,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Members));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MemberDelete,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Members));
                    },
                    customPolicyCallback);

                var handlers = new IAuthorizationHandler[]
                {
                    new UmbracoSectionAccessHandler(),
                    new ContentPermissionHandler(applicationContext.Services),
                    new MediaPermissionHandler(applicationContext.Services)
                };
                options.Dependencies.Service = new DefaultAuthorizationService(new DefaultAuthorizationPolicyProvider(options), handlers);

                app.CreatePerOwinContext(() => new AuthorizationServiceWrapper(options.Dependencies.Service));
            });
        }

        /// <summary>
        /// Used to enable back office cookie authentication for the REST API calls
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoCookieAuthenticationForRestApi(this IAppBuilder app, ApplicationContext appContext)
        {
            //Don't proceed if the app is not ready
            if (appContext.IsUpgrading == false && appContext.IsConfigured == false) return app;

            var authOptions = new UmbracoBackOfficeCookieAuthOptions(
                UmbracoConfig.For.UmbracoSettings().Security,
                GlobalSettings.TimeOutInMinutes,
                GlobalSettings.UseSSL)
            {
                Provider = new BackOfficeCookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user 
                    // logs in. This is a security feature which is used when you 
                    // change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator
                        .OnValidateIdentity<BackOfficeUserManager, BackOfficeIdentityUser, int>(
                            TimeSpan.FromMinutes(30),
                            (manager, user) => user.GenerateUserIdentityAsync(manager),
                            identity => identity.GetUserId<int>()),
                }
            };

            //This is what will ensure that the rest api calls are auth'd
            authOptions.CookieManager = new RestApiCookieManager();

            app.UseCookieAuthentication(authOptions);

            return app;
        }

        /// <summary>
        /// This will allow a custom callback to specify a custom policy if required, otherwise it will use the default policy
        /// </summary>
        /// <param name="options"></param>
        /// <param name="name"></param>
        /// <param name="configurePolicy"></param>
        /// <param name="customPolicyCallback"></param>
        public static void AddAuthorizationPolicy(
            AuthorizationOptions options,
            string name,
            Action<AuthorizationPolicyBuilder> configurePolicy,
            Func<string, AuthorizationPolicy, Action<AuthorizationPolicyBuilder>> customPolicyCallback)
        {
            //get the default policy
            var defaultPolicy = GetPolicy(configurePolicy);

            //check if there's a callback and use it
            if (customPolicyCallback != null)
            {
                var result = customPolicyCallback(name, defaultPolicy);
                if (result != null)
                {
                    options.AddPolicy(name, result);
                    return;
                }
            }

            //no custom callback or it didn't return a custom policy so use the default
            options.AddPolicy(name, defaultPolicy);
        }

        /// <summary>
        /// Returns a concrete policy from a builder
        /// </summary>
        /// <param name="configurePolicy"></param>
        /// <returns></returns>
        private static AuthorizationPolicy GetPolicy(Action<AuthorizationPolicyBuilder> configurePolicy)
        {
            if (configurePolicy == null) throw new ArgumentNullException(nameof(configurePolicy));
            var authorizationPolicyBuilder = new AuthorizationPolicyBuilder();
            configurePolicy(authorizationPolicyBuilder);
            return authorizationPolicyBuilder.Build();
        }
    }
}
