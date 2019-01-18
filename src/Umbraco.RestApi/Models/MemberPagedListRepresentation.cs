using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class MemberPagedListRepresentation : PagedListRepresentation<MemberRepresentation>
    {
        public MemberPagedListRepresentation(IList<MemberRepresentation> res, long totalResults, long totalPages, long page, int pageSize, Link uriTemplate, object uriTemplateSubstitutionParams) :
            base(res, totalResults, totalPages, page, pageSize, uriTemplate, uriTemplateSubstitutionParams)
        {
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            Href = LinkTemplates.Members.Root.Href;
            Links.Add(new Link("self", Href));

            Links.Add(LinkTemplates.Members.Root);
            Links.Add(LinkTemplates.Members.Search);

        }
    }
}
