using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Umbraco.Core;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class EntityRepresentation : Representation
    {
        protected EntityRepresentation()
        {
        }

        [Required]
        [Display(Name = "name")]
        public string Name { get; set; }
        
        [Display(Name = "parentId")]
        public Guid ParentId { get; set; }
        public string Path { get; set; }
        public bool HasChildren { get; set; }
        public int Level { get; set; }

        /// <summary>
        /// Used internally to avoid extra lookups
        /// </summary>
        [JsonIgnore]
        internal int InternalId { get; set; }

        /// <summary>
        /// The Guid for the entity
        /// </summary>
        /// <remarks>
        /// This is readonly
        /// </remarks>
        public Guid Id { get; set; }
        
        public int SortOrder { get; set; }

    }
}
