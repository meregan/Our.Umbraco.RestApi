namespace Umbraco.RestApi.Models
{
    public class PublishedContentRepresentation : ContentRepresentationBase
    {
        public string WriterName { get; set; }
        public string CreatorName { get; set; }
        public int WriterId { get; set; }
        public int CreatorId { get; set; }
        
        public string UrlName { get; set; }
        public string Url { get; set; }
        
        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //required link to self
            Href = LinkTemplates.PublishedContent.Self.CreateLink(new { id = Id }).Href;
            Rel = LinkTemplates.PublishedContent.Self.Rel;

            Links.Add(LinkTemplates.PublishedContent.Root);

            Links.Add( LinkTemplates.PublishedContent.PagedChildren.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.PublishedContent.PagedDescendants.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.PublishedContent.PagedAncestors.CreateLinkTemplate(Id));

            Links.Add(LinkTemplates.PublishedContent.Parent.CreateLink(new { parentId = ParentId }));
            
            //links to the relations api
            Links.Add(LinkTemplates.Relations.Children.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Relations.Parents.CreateLinkTemplate(Id));
        }
    }

    
}
