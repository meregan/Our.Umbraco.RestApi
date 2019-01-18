using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class ContentListRepresenation : SimpleListRepresentation<ContentRepresentation>
    {
        
        public ContentListRepresenation(IList<ContentRepresentation> items)
            : base(items)
        {
        }

        protected override void CreateHypermedia()
        {
            Href = LinkTemplates.Content.Root.Href;
            Links.Add(new Link("self", Href));

            Links.Add(LinkTemplates.Content.Root);
            Links.Add(LinkTemplates.Content.Search);

            Links.Add(LinkTemplates.Content.PagedChildren);
            Links.Add(LinkTemplates.Content.PagedDescendants);
            Links.Add(LinkTemplates.Content.PagedAncestors);
        }
    }
}
