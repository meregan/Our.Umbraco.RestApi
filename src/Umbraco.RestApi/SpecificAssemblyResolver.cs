using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace Umbraco.RestApi
{
    internal class SpecificAssemblyResolver : IAssembliesResolver
    {
        private readonly Assembly[] _assemblies;

        public SpecificAssemblyResolver(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public ICollection<Assembly> GetAssemblies()
        {
            return _assemblies;
        }
    }
}