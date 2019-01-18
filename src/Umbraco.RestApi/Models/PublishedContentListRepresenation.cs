using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models {

    public class PublishedContentListRepresenation : SimpleListRepresentation<PublishedContentRepresentation>
    {
       

        public PublishedContentListRepresenation(IList<PublishedContentRepresentation> items)
            : base(items)
        {
        }

        protected override void CreateHypermedia()
        {

            Href = LinkTemplates.PublishedContent.Root.Href;
            Links.Add(new Link("self", Href));
            Links.Add( LinkTemplates.PublishedContent.Root );

            Links.Add(LinkTemplates.PublishedContent.PagedChildren);
            Links.Add(LinkTemplates.PublishedContent.PagedDescendants);
            Links.Add(LinkTemplates.PublishedContent.PagedAncestors);


            Links.Add(LinkTemplates.PublishedContent.Query);
            Links.Add(LinkTemplates.PublishedContent.Search);
            Links.Add(LinkTemplates.PublishedContent.Url);
            Links.Add(LinkTemplates.PublishedContent.Tag);
        }
    }
}
