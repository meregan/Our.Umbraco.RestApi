using System;
using System.Web.Http;
using System.Web.Http.Routing;

namespace Umbraco.RestApi.Routing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class CustomRouteAttribute : Attribute, IDirectRouteFactory
    {
        public CustomRouteAttribute(string template)
        {
            InnerAttribute = new RouteAttribute(template);
        }

        public string Name
        {
            get => InnerAttribute.Name;
            set => InnerAttribute.Name = value;
        }

        public int Order
        {
            get => InnerAttribute.Order;
            set => InnerAttribute.Order = value;
        }

        public RouteAttribute InnerAttribute;

        RouteEntry IDirectRouteFactory.CreateRoute(DirectRouteFactoryContext context)
        {
            var result = ((IDirectRouteFactory)InnerAttribute).CreateRoute(context);

            var writeableResult = new RouteEntry(result.Name, new WriteableRoute(result.Route));

            //need to add this here so we can retrieve it later
            writeableResult.Route.DataTokens.Add("Umb_RouteName", Name);
            return writeableResult;
        }
    }
}