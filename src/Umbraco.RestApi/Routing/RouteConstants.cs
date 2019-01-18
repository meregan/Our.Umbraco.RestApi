using System.Reflection;

namespace Umbraco.RestApi.Routing
{
    public static class RouteConstants
    {
        public const string PublishedSegment = "published";
        public const string ContentSegment = "content";
        public const string MediaSegment = "media";
        public const string MembersSegment = "members";
        public const string RelationsSegment = "relations"; 

        public static string VersionSegment(int version)
        {
            return "v" + version;
        }

        public const string RestSegment = "rest";

        public const string PublishedContentRouteName = "UR_PublishedContent";
        public const string ContentRouteName = "UR_Content";
        public const string MediaRouteName = "UR_Media";
        public const string MembersRouteName = "UR_Members";
        public const string RelationsRouteName = "UR_Relations";

        public static string GetRestRootPath(int version)
        {
            return string.Format("{0}/{1}/{2}", UmbracoMvcArea, RestSegment, VersionSegment(version));
        }

        /// <summary>
        /// Gets the route name for the GET requests
        /// </summary>
        /// <param name="baseRouteName"></param>
        /// <returns></returns>
        public static string GetRouteNameForIdGetRequests(string baseRouteName)
        {
            return baseRouteName + "_1";
        }

        public static string GetRouteNameForSearchRequests(string baseRouteName)
        {
            return baseRouteName + "_2";
        }

        private static string _umbracoMvcArea;

        /// <summary>
        /// This returns the string of the MVC Area route.
        /// </summary>
        /// <remarks>
        /// Uses reflection to get the internal property in umb core, we don't want to expose this publicly in the core
        /// until we sort out the Global configuration bits and make it an interface, put them in the correct place, etc...
        /// </remarks>
        internal static string UmbracoMvcArea => _umbracoMvcArea ??
                                                 //Use reflection to get the type and value and cache
                                                 (_umbracoMvcArea = (string)Assembly.Load("Umbraco.Core").GetType("Umbraco.Core.Configuration.GlobalSettings").GetStaticProperty("UmbracoMvcArea"));
    }
}