using System;
using System.Web.Cors;
using Microsoft.Owin.Security.Authorization;

namespace Umbraco.RestApi
{
    public class UmbracoRestApiOptions
    {
        /// <summary>
        /// Default options allows all request, CORS does not limit anything
        /// </summary>
        public UmbracoRestApiOptions()
        {
            //These are the defaults that we know work with auth and the REST API
            // but people can modify them if required.
            CorsPolicy = new CorsPolicy()
            {
                AllowAnyOrigin = true,
                SupportsCredentials = true,
                AllowAnyHeader = true,
                Methods = {"GET", "POST", "DELETE", "PUT"}
            };
        }

        public CorsPolicy CorsPolicy { get; set; }

        /// <summary>
        /// If set this can be used to customize the Authorization policies applied to each controller/action
        /// </summary>
        public Func<string, AuthorizationPolicy, Action<AuthorizationPolicyBuilder>> CustomAuthorizationPolicyCallback { get; set; }
    }
}