using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public abstract class PagedListRepresentation<TRepresentation> : SimpleListRepresentation<TRepresentation> where TRepresentation : Representation
    {
        private readonly Link _uriTemplate;

        protected PagedListRepresentation(IList<TRepresentation> res, long totalResults, long totalPages, long page, int pageSize, Link uriTemplate, object uriTemplateSubstitutionParams)
            : base(res)
        {
            _uriTemplate = uriTemplate;
            TotalResults = totalResults;
            TotalPages = totalPages;
            Page = page;
            PageSize = pageSize;
            UriTemplateSubstitutionParams = uriTemplateSubstitutionParams;
        }

        public long TotalResults { get; set; }
        public long TotalPages { get; set; }
        public long Page { get; set; }
        public int PageSize { get; set; }

        protected object UriTemplateSubstitutionParams;

        protected override void CreateHypermedia()
        {
            var prms = new List<object> { new { page = Page, size = PageSize } };
            if (UriTemplateSubstitutionParams != null)
                prms.Add(UriTemplateSubstitutionParams);

            Href = Href ?? _uriTemplate.CreateLink(prms.ToArray()).Href;

            Links.Add(new Link { Href = Href, Rel = "self" });


            if (Page > 1)
            {
                var item = UriTemplateSubstitutionParams == null
                                ? _uriTemplate.CreateLink("prev", new { page = Page - 1, size = PageSize })
                                : _uriTemplate.CreateLink("prev", UriTemplateSubstitutionParams, new { page = Page - 1, size = PageSize }); // page overrides UriTemplateSubstitutionParams
                Links.Add(item);
            }

            if (Page < TotalPages)
            {
                var link = UriTemplateSubstitutionParams == null // kbr
                               ? _uriTemplate.CreateLink("next", new { page = Page + 1, size = PageSize })
                               : _uriTemplate.CreateLink("next", UriTemplateSubstitutionParams, new { page = Page + 1, size = PageSize }); // page overrides UriTemplateSubstitutionParams
                Links.Add(link);
            }

            Links.Add(new Link("page", _uriTemplate.Href));
        }
    }
}
