using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class MediaPagedListRepresentation : PagedListRepresentation<MediaRepresentation>
    {
        public MediaPagedListRepresentation(IList<MediaRepresentation> res, long totalResults, long totalPages, long page, int pageSize, Link uriTemplate, object uriTemplateSubstitutionParams) :
            base(res, totalResults, totalPages, page, pageSize, uriTemplate, uriTemplateSubstitutionParams)
        {
        }
    }
}
