using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class RelationTypeListRepresentation : SimpleListRepresentation<RelationTypeRepresentation>
    {
        
        public RelationTypeListRepresentation(IList<RelationTypeRepresentation> items)
            : base(items)
        {
           
        }

        protected override void CreateHypermedia()
        {
            Href = LinkTemplates.Relations.Root.Href;
            Links.Add(new Link("self", Href));

            Links.Add(LinkTemplates.Relations.Root);
            Links.Add(LinkTemplates.Relations.Children);
            Links.Add(LinkTemplates.Relations.Parents);
        }
    }
}
