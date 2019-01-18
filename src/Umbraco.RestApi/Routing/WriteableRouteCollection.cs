using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Http.Routing;

namespace Umbraco.RestApi.Routing
{
    /// <summary>
    /// This is the same as <see cref="WriteableRoute"/> but this implements <see cref="IEnumerable{IHttpRoute}"/> in order
    /// to expose the sub-routes to the WebApi <see cref="IApiExplorer"/>
    /// </summary>
    /// <remarks>
    /// The IApiExplorer is used by Swagger so without having this implement IEnumerable{IHttpRoute} then Swagger will not detect
    /// the custom routes.
    /// </remarks>
    internal class WriteableRouteCollection : WriteableRoute, IEnumerable<IHttpRoute>
    {
        private readonly IEnumerable<IHttpRoute> _routeCollection;

        public WriteableRouteCollection(IHttpRoute innerRoute) : base(innerRoute)
        {
            if (innerRoute == null)
                throw new ArgumentNullException(nameof(innerRoute));
            
            if (!(innerRoute is IEnumerable<IHttpRoute>))
                throw new InvalidOperationException("The route specified must be IEnumerable<IHttpRoute>");
            _routeCollection = (IEnumerable<IHttpRoute>)innerRoute;
        }

        public IEnumerator<IHttpRoute> GetEnumerator()
        {
            return _routeCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _routeCollection.GetEnumerator();
        }
    }
}