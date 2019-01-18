using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class MemberListRepresenation : SimpleListRepresentation<MemberRepresentation>
    {


        public MemberListRepresenation(IList<MemberRepresentation> items)
            : base(items)
        {
        }

        protected override void CreateHypermedia()
        {

            Href = LinkTemplates.Members.Root.Href;
            Links.Add(new Link("self", Href));
            Links.Add(LinkTemplates.Members.Root);
            Links.Add(LinkTemplates.Members.Search);
        }
    }
}