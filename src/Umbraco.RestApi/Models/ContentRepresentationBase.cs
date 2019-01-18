using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.RestApi.Serialization;

namespace Umbraco.RestApi.Models
{
    /// <summary>
    /// Used for Content and Media
    /// </summary>
    public abstract class ContentRepresentationBase : EntityRepresentation
    {
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
     
        [Required]
        [Display(Name = "contentTypeAlias")]
        public string ContentTypeAlias { get; set; }

        [JsonConverter(typeof(ExplicitlyCasedDictionaryKeyJsonConverter<object>))]
        public IDictionary<string, object> Properties { get; set; }
    }

}