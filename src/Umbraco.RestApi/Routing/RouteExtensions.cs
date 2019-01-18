using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace Umbraco.RestApi.Routing
{
    internal static class RouteExtensions
    {
        /// <summary>
        /// This will create attribute routes for specific controllers without interfering with normal attribute routing
        /// </summary>
        /// <param name="originalConfig"></param>
        /// <param name="routeNamePrefix">
        /// For non-named attributed routes, they are created as one master route containing sub-routes, this will be the 
        /// prefix name for that route, the suffix will be the controller type.
        /// For named attribute routes, those names will be used.
        /// </param>
        /// <param name="controllerTypes">
        /// Route these controllers based on their attributes. They will be routed in order (precedence)
        /// </param>
        /// <param name="subRouteCallback">
        /// A callback to modify the sub route (individual action routes)
        /// </param>
        /// <param name="mainRouteCallback">
        /// A callback to modify the main route (the one that the RouteTable has visibility for) 
        /// </param>
        /// <param name="inheritedAttributes"></param>
        public static void MapControllerAttributeRoutes(
            this HttpConfiguration originalConfig, 
            string routeNamePrefix,
            IEnumerable<Type> controllerTypes,
            Action<WriteableRoute> subRouteCallback = null,
            Action<WriteableRoute> mainRouteCallback = null,
            bool inheritedAttributes = false)
        {
            foreach (var controllerType in controllerTypes)
            {
                //new temp config to clone
                var tempConfig = new HttpConfiguration();

                tempConfig.Services.Replace(typeof(IHttpControllerTypeResolver), new SpecificControllerTypeResolver(new[] { controllerType }));
                tempConfig.MapHttpAttributeRoutes(new CustomRouteAttributeDirectRouteProvider(inheritedAttributes));

                var isInitialized = false;

                var originalInit = originalConfig.Initializer;
                var controllerTypeLocal = controllerType;

                originalConfig.Initializer = configuration =>
                {
                    //Track a boolean because otherwise we'll end up in an infinite loop because
                    // of the ctor of the HttpControllerDescriptor below will clone http config
                    // per controller and invoke the original initializer, and we don't want to
                    // initialize the clones again with the attribute routes.
                    if (!isInitialized)
                    {
                        isInitialized = true;

                        tempConfig.EnsureInitialized();

                        var controllerDescriptors = new List<HttpControllerDescriptor>();

                        //get the routes created
                        foreach (var attributeRoute in tempConfig.Routes)
                        {
                            //attributeRoute.Handler = GetMessageHandler()

                            //in many cases it's a collection of routes contained in a single route
                            var routeCollection = attributeRoute as IEnumerable<IHttpRoute>;
                            if (routeCollection != null)
                            {
                                //update each attribute route's action descriptor http configuration property
                                //to be the real configuration 
                                foreach (var httpRoute in routeCollection)
                                {
                                    SetDescriptorsOnRoute(httpRoute, configuration, controllerDescriptors);

                                    //callback can modify the route
                                    if (subRouteCallback != null)
                                    {
                                        subRouteCallback((WriteableRoute)httpRoute);
                                    }
                                }

                                //map the route into a writable route
                                var writeableRoute = new WriteableRouteCollection(attributeRoute);

                                if (mainRouteCallback != null)
                                {
                                    mainRouteCallback(writeableRoute);
                                }

                                //now we need to add the route back to the main configuration
                                originalConfig.Routes.Add(
                                    string.Format("{0}{1}", routeNamePrefix, controllerTypeLocal.FullName),
                                    writeableRoute);
                            }
                            else
                            {
                                SetDescriptorsOnRoute(attributeRoute, configuration, controllerDescriptors);
                                //NOTE: We cannot modify this route because it is a linking route - it
                                // it used only for generating links, not routing them

                                //now we need to add the route back to the main configuration
                                originalConfig.Routes.Add(
                                    attributeRoute.DataTokens["Umb_RouteName"].ToString(),
                                    attributeRoute);
                            }
                           
                        }
                    }

                    originalInit(configuration);
                };
            }
        }

        private static void SetDescriptorsOnRoute(IHttpRoute route, HttpConfiguration configuration, ICollection<HttpControllerDescriptor> controllerDescriptors)
        {
            var actionDescriptors = route.DataTokens["actions"] as IEnumerable<HttpActionDescriptor>;
            if (actionDescriptors != null)
            {
                foreach (var descriptor in actionDescriptors)
                {
                    descriptor.Configuration = configuration;

                    //IMPORTANT: We are making a new instance of a HttpControllerDescriptor to force
                    // the descriptor to initialize again so that IControllerConfiguration executes.
                    // if they are not the same webapi will throw an exception about ambiguous controllers.

                    //for any controller type, we need to have the exact same controller descriptor instance

                    var found = controllerDescriptors.SingleOrDefault(x => x.ControllerType == descriptor.ControllerDescriptor.ControllerType);
                    if (found == null)
                    {
                        found = new HttpControllerDescriptor(
                            configuration,
                            descriptor.ControllerDescriptor.ControllerName,
                            descriptor.ControllerDescriptor.ControllerType);
                        controllerDescriptors.Add(found);
                    }

                    descriptor.ControllerDescriptor = found;
                }
            }
        }

    }

    

}