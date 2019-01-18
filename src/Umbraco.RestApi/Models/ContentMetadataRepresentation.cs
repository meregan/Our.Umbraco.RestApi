using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.RestApi.Serialization;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class ContentMetadataRepresentation : Representation
    {
        private readonly Link _selfLink;
        private readonly Link _contentLink;

        public ContentMetadataRepresentation(Link selfLink, Link contentLink, Guid id)
        {
            _selfLink = selfLink;
            _contentLink = contentLink;

            Id = id;
            Fields = new Dictionary<string, ContentPropertyInfo>();
            Properties = new Dictionary<string, ContentPropertyInfo>();
        }
        
        public Guid Id { get; set; }

        /// <summary>
        /// If the model supports creating, then this is it's template, null means it does not support creating
        /// </summary>
        public ContentCreationTemplate CreateTemplate { get; set; }

        public IDictionary<string, ContentPropertyInfo> Fields { get; set; }

        [JsonConverter(typeof(ExplicitlyCasedDictionaryKeyJsonConverter<ContentPropertyInfo>))]
        public IDictionary<string, ContentPropertyInfo> Properties { get; set; }

        public override string Rel
        {
            get
            {
                if (_selfLink == null) throw new NullReferenceException("LinkTemplate is null");
                return _selfLink.Rel;
            }
            set => throw new NotSupportedException();
        }

        public override string Href
        {
            get
            {
                if (_selfLink == null) throw new NullReferenceException("LinkTemplate is null");
                return _selfLink.CreateLink(new { id = Id }).Href;
            }
            set => throw new NotSupportedException();
        }

        protected override void CreateHypermedia()
        {
            if (_contentLink == null) throw new NullReferenceException("LinkTemplate is null");
            Links.Add(_contentLink.CreateLink(new { id = Id }));      
        }
    }
}