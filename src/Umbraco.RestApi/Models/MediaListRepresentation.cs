using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class MediaListRepresenation : SimpleListRepresentation<MediaRepresentation>
    {


        public MediaListRepresenation(IList<MediaRepresentation> items)
            : base(items)
        {
        }

        protected override void CreateHypermedia()
        {
            Href = LinkTemplates.Media.Root.Href;
            Links.Add(new Link("self", Href));

            Links.Add(LinkTemplates.Media.Root);
            Links.Add(LinkTemplates.Media.Search);

            Links.Add(LinkTemplates.Media.PagedChildren);
            Links.Add(LinkTemplates.Media.PagedDescendants);

            Links.Add(LinkTemplates.Media.Upload);
        }
    }
}
