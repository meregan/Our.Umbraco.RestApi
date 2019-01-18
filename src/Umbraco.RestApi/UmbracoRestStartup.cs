using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Umbraco.Core;
using Umbraco.RestApi.Controllers;
using Umbraco.RestApi.Routing;

namespace Umbraco.RestApi
{
    public class UmbracoRestStartup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Create routes for REST
            CreateRoutes(GlobalConfiguration.Configuration, new[]
            {
                typeof(PublishedContentController),
                typeof(ContentController),
                typeof(MediaController),
                typeof(MembersController),
                typeof(RelationsController)
            });
        }
        

        public static void CreateRoutes(HttpConfiguration config, params Type[] halControllerTypes)
        {
            //NOTE : we are using custom attribute routing... This is because there is no way to enable attribute routing against your own
            // assemblies with a custom DefaultDirectRouteProvider which we would require to implement inherited attribute routes. 
            // So we've gone ahead and made this possible. It doesn't use any reflection, just a few tricks and works quite well. 
            // We just need to use the CustomRouteAttribute instead of the normal RouteAttribute.
            config.MapControllerAttributeRoutes(
                routeNamePrefix: "UmbRest-",
                //Map these explicit controllers in the order they appear
                controllerTypes: halControllerTypes,                
                mainRouteCallback: route =>
                {
                    if (route.DataTokens == null) route.DataTokens = new Dictionary<string, object>();
                    route.DataTokens["Namespaces"] = new[] { typeof(ContentController).Namespace };
                    route.DataTokens["UseNamespaceFallback"] = false;
                    route.Handler = GetMessageHandler(config);
                },
                inheritedAttributes: true);


        }

        /// <summary>
        /// Gets a custom message handler that explicitly contains the CorsMessageHandler for use 
        /// with our RestApi routes.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static HttpMessageHandler GetMessageHandler(HttpConfiguration config)
        {
            // Create a message handler chain with an end-point.
            return HttpClientFactory.CreatePipeline(
                new HttpControllerDispatcher(config),
                new DelegatingHandler[]
                {
                    //Explicitly include the CorsMessage handler!
                    // we're doing this so people don't have to do EnableCors() in their startup,
                    // we don't care about that, we always want to support Cors for the rest api
                    new CorsMessageHandler(config)
                });
        }

    }
}
