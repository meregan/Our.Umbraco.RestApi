using System.Reflection;
using System.Web.Http;
using Umbraco.Core;

namespace Umbraco.RestApi.Routing
{
    /// <summary>
    /// A custom route prefix which is based on the configured back office route/folder (i.e. Umbraco)
    /// </summary>
    public sealed class UmbracoRoutePrefixAttribute : RoutePrefixAttribute
    {
        public UmbracoRoutePrefixAttribute(string prefix)
            : base(prefix)
        {
            _fromConfig = true;
        }

        public UmbracoRoutePrefixAttribute(string prefix, string backofficeRoute)
            : base(prefix)
        {
            _fromConfig = false;
            _umbracoMvcArea = backofficeRoute;
        }

        /// <summary>
        /// Gets the prefix with the umbraco back office configured route 
        /// </summary>
        public override string Prefix => UmbracoMvcArea.EnsureEndsWith('/') + base.Prefix.TrimEnd('/');

        private readonly bool _fromConfig;
        private string _umbracoMvcArea;

        /// <summary>
        /// This returns the string of the MVC Area route.
        /// </summary>
        /// <remarks>
        /// Uses reflection to get the internal property in umb core, we don't want to expose this publicly in the core
        /// until we sort out the Global configuration bits and make it an interface, put them in the correct place, etc...
        /// </remarks>
        internal string UmbracoMvcArea
        {
            get
            {
                if (_fromConfig)
                {
                    return _umbracoMvcArea ??
                           //Use reflection to get the type and value and cache
                           (_umbracoMvcArea = (string)Assembly.Load("Umbraco.Core").GetType("Umbraco.Core.Configuration.GlobalSettings").GetStaticProperty("UmbracoMvcArea"));
                }
                return _umbracoMvcArea;
            }
        }
    }
}