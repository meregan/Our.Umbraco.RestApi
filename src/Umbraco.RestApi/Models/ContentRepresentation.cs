using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.RestApi.Models
{
    public class ContentRepresentation : ContentRepresentationBase
    {
        public ContentRepresentation()
        {
        }
        
        [Display(Name = "templateId")]
        public Guid TemplateId { get; set; }

        [Required]
        [Display(Name = "published")]
        public bool Published { get; set; }
        
        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //required link to self
            Href = LinkTemplates.Content.Self.CreateLink(new { id = Id }).Href;
            Rel = LinkTemplates.Content.Self.Rel;

            Links.Add(LinkTemplates.Content.Root);

            Links.Add(LinkTemplates.Content.PagedChildren.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Content.PagedDescendants.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Content.PagedAncestors.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Content.Parent.CreateLink(new { parentId = ParentId }));

            //links to the relations api
            Links.Add(LinkTemplates.Relations.Children.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Relations.Parents.CreateLinkTemplate(Id));

            //file upload
            Links.Add(LinkTemplates.Media.Upload.CreateLink(new { id = Id }));
        }
    }
}
