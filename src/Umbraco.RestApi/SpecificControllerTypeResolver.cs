using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dispatcher;

namespace Umbraco.RestApi
{
    internal class SpecificControllerTypeResolver : IHttpControllerTypeResolver
    {
        private readonly IEnumerable<Type> _controllerTypes;

        public SpecificControllerTypeResolver(IEnumerable<Type> controllerTypes)
        {
            if (controllerTypes == null) throw new ArgumentNullException("controllerTypes");
            _controllerTypes = controllerTypes;
        }

        /// <summary>
        /// Returns a list of controllers available for the application. 
        /// </summary>
        /// <returns>
        /// An &lt;see cref="T:System.Collections.Generic.ICollection`1" /&gt; of controllers.
        /// </returns>
        /// <param name="assembliesResolver">The resolver for failed assemblies.</param>
        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return _controllerTypes.ToList();
        }
    }
}