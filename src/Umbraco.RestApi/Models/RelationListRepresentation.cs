using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class RelationListRepresentation : SimpleListRepresentation<RelationRepresentation>
    {

        public RelationListRepresentation(IList<RelationRepresentation> items)
            : base(items)
        {

        }

        protected override void CreateHypermedia()
        {
            Href = LinkTemplates.Relations.Root.Href;
            Links.Add(new Link("self", Href));
            Links.Add(LinkTemplates.Relations.Root);

        }
    }
}
