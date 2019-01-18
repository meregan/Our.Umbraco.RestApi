using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class RelationTypeRepresentation : Representation
    {
        [Required]
        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "alias")]
        public string Alias { get; set; }
 
        [Display(Name = "bidirectional")]
        public bool IsBidirectional { get; set; }

        //TODO: relations can be between more than these types
        [Display(Name = "parentEntityType")]
        public PublishedItemType ParentEntityType { get; set; }

        //TODO: relations can be between more than these types
        [Display(Name = "childEntityType")]
        public PublishedItemType ChildEntityType { get; set; }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //required link to self
            Href = LinkTemplates.Relations.RelationType.CreateLink(new { alias = Alias }).Href;
            Rel = LinkTemplates.Relations.RelationType.Rel;

            Links.Add(LinkTemplates.Media.Root);
        }
    }
}
