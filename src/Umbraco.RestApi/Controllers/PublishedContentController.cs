using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Examine;
using Examine.Providers;
using Microsoft.Owin.Security.Authorization.WebApi;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Models.Mapping;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Security;
using Umbraco.Web;

namespace Umbraco.RestApi.Controllers
{
    [ResourceAuthorize(Policy = AuthorizationPolicies.PublishedContentRead)]
    [UmbracoRoutePrefix("rest/v1/content/published")]
    // Method overload warnings can be ignored - CustomRoute attributes are handling it.
    [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
    public class PublishedContentController : UmbracoHalController
    {
        public PublishedContentController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="searchProvider"></param>
        /// <param name="pcrFactory"></param>
        public PublishedContentController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper,
            BaseSearchProvider searchProvider,
            IPublishedContentRequestFactory pcrFactory)
            : base(umbracoContext, umbracoHelper)
        {
            _pcrFactory = pcrFactory;
            _searchProvider = searchProvider ?? throw new ArgumentNullException("searchProvider");
        }

        private IPublishedContentRequestFactory _pcrFactory;
        protected IPublishedContentRequestFactory PcrFactory => _pcrFactory ?? (_pcrFactory = new PublishedContentRequestFactory(UmbracoContext, UmbracoConfig.For.UmbracoSettings().WebRouting, Roles.GetRolesForUser));

        private BaseSearchProvider _searchProvider;
        protected BaseSearchProvider SearchProvider => _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["ExternalSearcher"]);

        [HttpGet]
        [CustomRoute("")]
        public virtual HttpResponseMessage Get(int depth = PublishedContentMapper.DefaultDepth)
        {
            var rootContent = Umbraco.TypedContentAtRoot().ToArray();
            if (rootContent.Length > 0)
                PcrFactory.Create(rootContent[0], Request.RequestUri);

            var result = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(rootContent, options => options.Items["prop::depth"] = depth).ToList();
            var representation = new PublishedContentListRepresenation(result);
            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("{id:int}")]
        public HttpResponseMessage Get(int id, int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetInternal(() => Umbraco.TypedContent(id), depth);
        }

        [HttpGet]
        [CustomRoute("{id:guid}")]
        public HttpResponseMessage Get(Guid id, int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetInternal(() => Umbraco.TypedContent(id), depth);
        }

        private HttpResponseMessage GetInternal(Func<IPublishedContent> getContent, int depth)
        {
            var content = getContent();
            if (content == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            PcrFactory.Create(content, Request.RequestUri);

            var result = AutoMapper.Mapper.Map<PublishedContentRepresentation>(content, options => options.Items["prop::depth"] = depth);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id:int}/children")]
        public HttpResponseMessage GetChildren(int id,
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query,
            int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetChildren(() => Umbraco.TypedContent(id), query, depth);
        }

        [HttpGet]
        [CustomRoute("{id:guid}/children")]
        public HttpResponseMessage GetChildren(Guid id,
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))] PagedQuery query, int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetChildren(() => Umbraco.TypedContent(id), query, depth);
        }

        private HttpResponseMessage GetChildren(Func<IPublishedContent> getContent, PagedQuery query, int depth)
        {
            var content = getContent();
            if (content == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            PcrFactory.Create(content, Request.RequestUri);

            var resolved = (string.IsNullOrEmpty(query.Query)) ? content.Children().ToArray() : content.Children(query.Query.Split(',')).ToArray();
            var total = resolved.Length;
            var pages = (total + query.PageSize - 1) / query.PageSize;

            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(resolved
                    .Skip(ContentControllerHelper.GetSkipSize(query.Page - 1, query.PageSize))
                    .Take(query.PageSize),
                options => options.Items["prop::depth"] = depth).ToList();
            var result = new PublishedContentPagedListRepresentation(items, total, pages, query.Page, query.PageSize, LinkTemplates.PublishedContent.PagedChildren, new { id = content.GetKey() });

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id:int}/descendants/")]
        public HttpResponseMessage GetDescendants(int id,
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query,
            int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetDescendantsInternal(() => Umbraco.TypedContent(id), query, depth);
        }

        [HttpGet]
        [CustomRoute("{id:guid}/descendants/")]
        public HttpResponseMessage GetDescendants(Guid id,
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))] PagedQuery query, int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetDescendantsInternal(() => Umbraco.TypedContent(id), query, depth);
        }

        private HttpResponseMessage GetDescendantsInternal(Func<IPublishedContent> getContent, PagedQuery query, int depth)
        {
            var content = getContent();
            if (content == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            PcrFactory.Create(content, Request.RequestUri);

            var resolved = (string.IsNullOrEmpty(query.Query)) ? content.Descendants().ToArray() : content.Descendants(query.Query).ToArray();

            var total = resolved.Length;
            var pages = (total + query.PageSize - 1) / query.PageSize;
            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(resolved
                    .Skip(ContentControllerHelper.GetSkipSize(query.Page - 1, query.PageSize))
                    .Take(query.PageSize),
                options => options.Items["prop::depth"] = depth).ToList();
            var result = new PublishedContentPagedListRepresentation(items, total, pages, query.Page, query.PageSize, LinkTemplates.PublishedContent.PagedDescendants, new { id = content.GetKey() });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id:int}/ancestors/{page?}/{pageSize?}")]
        public HttpResponseMessage GetAncestors(int id,
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query,
            int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetAncestorsInternal(() => Umbraco.TypedContent(id), query, depth);
        }

        [HttpGet]
        [CustomRoute("{id:guid}/ancestors/{page?}/{pageSize?}")]
        public HttpResponseMessage GetAncestors(Guid id,
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))] PagedQuery query,
            int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetAncestorsInternal(() => Umbraco.TypedContent(id), query, depth);
        }

        private HttpResponseMessage GetAncestorsInternal(Func<IPublishedContent> getContent, PagedQuery query, int depth)
        {
            var content = getContent();
            if (content == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            PcrFactory.Create(content, Request.RequestUri);

            var resolved = (string.IsNullOrEmpty(query.Query)) ? content.Ancestors().ToArray() : content.Ancestors(query.Query).ToArray();

            var total = resolved.Length;
            var pages = (total + query.PageSize - 1) / query.PageSize;

            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(resolved
                    .Skip(ContentControllerHelper.GetSkipSize(query.Page - 1, query.PageSize))
                    .Take(query.PageSize),
                options => options.Items["prop::depth"] = depth).ToList();
            var result = new PublishedContentPagedListRepresentation(items, total, pages, query.Page, query.PageSize, LinkTemplates.PublishedContent.PagedAncestors, new { id = content.GetKey() });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("query")]
        public HttpResponseMessage GetQuery(
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query,
            int depth = PublishedContentMapper.DefaultDepth)
        {
            return GetQuery(query, 0, depth);
        }

        [HttpGet]
        [CustomRoute("query/{id:int}")]
        public HttpResponseMessage GetQuery(
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query,
            int id,
            int depth = PublishedContentMapper.DefaultDepth)
        {
            var rootQuery = "";
            if (id > 0)
            {
                //TODO: Change to xpath id() query, see https://github.com/umbraco/Umbraco-CMS/pull/1831
                rootQuery = $"//*[@id='{id}']";
            }

            var skip = (query.Page - 1) * query.PageSize;
            var take = query.PageSize;

            var result = new IPublishedContent[0];

            try
            {
                result = Umbraco.TypedContentAtXPath(rootQuery + query.Query).ToArray();
            }
            catch (Exception)
            {
                //in case the xpath query fails - do nothing as we will return a empty array instead
            }

            var key = Umbraco.TypedContent(id)?.GetKey();

            var paged = result.Skip((int)skip).Take(take);
            var pages = (result.Length + query.PageSize - 1) / query.PageSize;
            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(paged, options => options.Items["prop::depth"] = depth).ToList();
            var representation = new PublishedContentPagedListRepresentation(items, result.Length, pages, query.Page, query.PageSize, LinkTemplates.PublishedContent.Query, new { id = key, query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("query/{id:guid}")]
        public HttpResponseMessage GetQuery(
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))] PagedQuery query,
            Guid id,
            int depth = PublishedContentMapper.DefaultDepth)
        {
            //we will convert to INT here because the GUID lookup in xpath is slow until we fix this https://github.com/umbraco/Umbraco-CMS/pull/2367
            var intId = id != Guid.Empty ? Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Document) : Attempt.Succeed(0);
            if (intId.Result <= 0)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return GetQuery(query, intId.Result);
        }

        [HttpGet]
        [CustomRoute("search")]
        public HttpResponseMessage Search(
            [System.Web.Http.ModelBinding.ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query,
            int depth = PublishedContentMapper.DefaultDepth)
        {
            if (query.Query.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

            //TODO: This would be WAY more efficient if we went straight to the ExamineManager and used it's built in Skip method
            // but then we have to write our own model mappers and don't have time for that right now.
            //see https://github.com/umbraco/UmbracoRestApi/issues/25

            var result = Umbraco.ContentQuery.TypedSearch(SearchProvider.CreateSearchCriteria().RawQuery(query.Query), _searchProvider).ToArray();
            var paged = result.Skip(ContentControllerHelper.GetSkipSize(query.Page - 1, query.PageSize)).Take(query.PageSize);
            var pages = (result.Length + query.PageSize - 1) / query.PageSize;
            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(paged, options => options.Items["prop::depth"] = depth).ToList();
            var representation = new PublishedContentPagedListRepresentation(items, result.Length, pages, query.Page, query.PageSize, LinkTemplates.PublishedContent.Search, new { query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("url")]
        public HttpResponseMessage GetByUrl(string url, int depth = PublishedContentMapper.DefaultDepth)
        {
            var content = UmbracoContext.ContentCache.GetByRoute(url);
            if (content == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            PcrFactory.Create(content, Request.RequestUri);

            var result = AutoMapper.Mapper.Map<PublishedContentRepresentation>(content, options => options.Items["prop::depth"] = depth);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("tag/{tag}")]
        public HttpResponseMessage GetByTag(string tag, string group = null, int page = 0, int size = 100, int depth = PublishedContentMapper.DefaultDepth)
        {
            var content = Umbraco.TagQuery.GetContentByTag(tag, group).ToArray();

            if (content.Length > 0)
                PcrFactory.Create(content[0], Request.RequestUri);

            var skip = (page * size);
            var total = content.Length;
            var pages = (total + size - 1) / size;

            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(content.Skip(skip).Take(size), options => options.Items["prop::depth"] = depth).ToList();
            var representation = new PublishedContentPagedListRepresentation(items, total, pages, page, size, LinkTemplates.PublishedContent.Search, new
            {
                tag = tag,
                group = group,
                page = page,
                size = size
            });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }
    }
}
