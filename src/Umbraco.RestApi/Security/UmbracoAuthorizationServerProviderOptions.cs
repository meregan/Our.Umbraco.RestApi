using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Cors;
using System.Web.Hosting;
using ClientDependency.Core;
using Microsoft.AspNet.Identity.Owin;

namespace Umbraco.RestApi.Security
{
    public class UmbracoAuthorizationServerProviderOptions
    {

        /// <summary>
        /// Default options allows all request, CORS does not limit anything
        /// </summary>
        public UmbracoAuthorizationServerProviderOptions()
        {
            //These are the defaults that we know work but people can modify them
            // on startup if required.
            CorsPolicy = new CorsPolicy()
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = true,
                SupportsCredentials = true
            };

            //set the secret based on the current website

            var timezone = TimeZone.CurrentTimeZone.StandardName;
            var directory = HostingEnvironment.IsHosted ? HttpRuntime.BinDirectory : AppDomain.CurrentDomain.BaseDirectory + "/bin";            
            var machineName = Environment.MachineName;
            var appDomainAppId = HttpRuntime.AppDomainAppId;
            Secret = string.Concat(timezone, directory, machineName, appDomainAppId).GenerateHash();

            //for now we only have one Audience
            Audience = "UmbracoRestApi";
        }

        /// <summary>
        /// Generally you wouldn't allow this unless on SSL!
        /// </summary>
        public bool AllowInsecureHttp { get; set; }

        //TODO: The Secret and Audience need to be turned into an IAudienceStore or IClientStore 
        //Need to follow more of this for http://bitoftech.net/2014/10/27/json-web-token-asp-net-web-api-2-jwt-owin-authorization-server/ dealing with AudienceStores

        /// <summary>
        /// This is the key for the client
        /// </summary>
        internal string Secret { get; set; }

        /// <summary>
        /// This is a "ClientId"
        /// </summary>
        internal string Audience { get; set; }

        public string AuthEndpoint { get; set; } = "/umbraco/rest/oauth/token";
        public CorsPolicy CorsPolicy { get; set; }
    }
}
