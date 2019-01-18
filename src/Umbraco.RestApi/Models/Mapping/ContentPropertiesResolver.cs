using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;

namespace Umbraco.RestApi.Models.Mapping
{
    internal class ContentPropertiesResolver : ValueResolver<IContentBase, IDictionary<string, object>>
    {
        protected override IDictionary<string, object> ResolveCore(IContentBase content)
        {
            var d = new Dictionary<string, object>();
            foreach (var propertyType in content.PropertyTypes)
            {
                var prop = content.HasProperty(propertyType.Alias) ? content.Properties[propertyType.Alias] : null;
                if (prop != null)
                {
                    d.Add(propertyType.Alias, prop.Value);
                }
            }
            return d;
        }
    }
}