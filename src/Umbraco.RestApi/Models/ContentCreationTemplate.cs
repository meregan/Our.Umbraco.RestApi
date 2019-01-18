using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.RestApi.Serialization;

namespace Umbraco.RestApi.Models
{
    /// <summary>
    /// If the model supports creating, then this is it's template
    /// </summary>
    public class ContentCreationTemplate
    {        
        public string ContentTypeAlias { get; set; }
        public Guid ParentId { get; set; }
        public Guid TemplateId { get; set; }
        public string Name { get; set; }

        [JsonConverter(typeof(ExplicitlyCasedDictionaryKeyJsonConverter<object>))]
        public IDictionary<string, object> Properties { get; set; }
    }
}