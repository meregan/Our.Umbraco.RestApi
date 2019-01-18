using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace Umbraco.RestApi.Routing
{
    /// <summary>
    /// This is used to lookup our CustomRouteAttribute instead of the normal RouteAttribute so that 
    /// we can use the CustomRouteAttribute instead of the RouteAttribute on our controlles so the normal
    /// MapHttpAttributeRoutes method doesn't try to route our controllers - since the point of this is
    /// to be able to map our controller routes with attribute routing explicitly without interfering
    /// with normal developers usages.
    /// </summary>
    internal class CustomRouteAttributeDirectRouteProvider : DefaultDirectRouteProvider
    {
        private readonly bool _inherit;

        public CustomRouteAttributeDirectRouteProvider(bool inherit = false)
        {
            _inherit = inherit;
        }

        protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetCustomAttributes<CustomRouteAttribute>(inherit: _inherit);
        }
    }
}