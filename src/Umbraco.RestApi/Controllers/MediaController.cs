using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Examine;
using Examine.Providers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;
using System.Web.Http.ModelBinding;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Authorization.WebApi;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.RestApi.Security;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// A controller for working with media (database level)
    /// </summary>
    /// <remarks>
    /// TODO: Query access to this controller will generally only work if the Id claim type belongs to a real Umbraco User since permissions
    /// for that user need to be looked up. The only way around this would be to be able to have an IPermissionService that could be added
    /// to the rest api options and a developer could replace that.
    /// </remarks>
    [ResourceAuthorize(Policy = AuthorizationPolicies.DefaultRestApi)]
    [UmbracoRoutePrefix("rest/v1/media")]
    public class MediaController : UmbracoHalController, ITraversableController<int, MediaRepresentation>, ITraversableController<Guid, MediaRepresentation>
    {
        
        /// <summary>
        /// Default ctor
        /// </summary>
        public MediaController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="searchProvider"></param>
        /// <param name="contentSectionConfig"></param>
        public MediaController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper,
            BaseSearchProvider searchProvider,
            IContentSection contentSectionConfig)
            : base(umbracoContext, umbracoHelper)
        {
            if (searchProvider == null) throw new ArgumentNullException("searchProvider");
            _searchProvider = searchProvider;
            _contentSectionConfig = contentSectionConfig;
        }

        private BaseSearchProvider _searchProvider;
        private IContentSection _contentSectionConfig;

        protected IContentSection ContentSectionConfig => _contentSectionConfig ?? (_contentSectionConfig = UmbracoConfig.For.UmbracoSettings().Content);
        protected BaseSearchProvider SearchProvider => _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["ExternalSearcher"]);

        [HttpGet]
        [CustomRoute("")]
        public virtual async Task<HttpResponseMessage> Get()
        {
            var startMediaIdsAsInt = ClaimsPrincipal.GetMediaStartNodeIds();
            if (startMediaIdsAsInt == null || startMediaIdsAsInt.Length == 0)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(startMediaIdsAsInt), AuthorizationPolicies.MediaRead))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var rootMedia = Services.MediaService.GetRootMedia();
            var result = Mapper.Map<IEnumerable<MediaRepresentation>>(rootMedia).ToList();
            var representation = new MediaListRepresenation(result);

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("{id:int}")]
        public async Task<HttpResponseMessage> Get(int id)
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaRead))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var content = Services.MediaService.GetById(id);
            var result = Mapper.Map<MediaRepresentation>(content);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id:guid}")]
        public async Task<HttpResponseMessage> Get(Guid id)
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await Get(intId.Result);
        }

        [HttpGet]
        [CustomRoute("{id:int}/meta")]
        public async Task<HttpResponseMessage> GetMetadata(int id)
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaRead))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var found = Services.MediaService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var helper = new ContentControllerHelper(Services.TextService);

            var result = new ContentMetadataRepresentation(LinkTemplates.Media.MetaData, LinkTemplates.Media.Self, found.Key)
            {
                Fields = helper.GetDefaultFieldMetaData(ClaimsPrincipal),
                Properties = Mapper.Map<IDictionary<string, ContentPropertyInfo>>(found),
                CreateTemplate = Mapper.Map<ContentCreationTemplate>(found)
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id:guid}/meta")]
        public async Task<HttpResponseMessage> GetMetadata(Guid id)
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await GetMetadata(intId.Result);
        }

        [HttpGet]
        [CustomRoute("{id:int}/children")]
        public async Task<HttpResponseMessage> GetChildren(int id,
            [ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query)
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaRead))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var items = Services.MediaService.GetPagedChildren(id, query.Page - 1, query.PageSize, out var total, filter: query.Query);
            var pages = ContentControllerHelper.GetTotalPages(total, query.PageSize);
            var mapped = Mapper.Map<IEnumerable<MediaRepresentation>>(items).ToList();

            // this seems stupid since we usually end up in here by request via guid from the other overload...
            var key = Services.EntityService.GetKeyForId(id, UmbracoObjectTypes.Media);
            if (key.Result == Guid.Empty)
                Request.CreateResponse(HttpStatusCode.NotFound);

            var result = new MediaPagedListRepresentation(mapped, total, pages, query.Page, query.PageSize, LinkTemplates.Media.PagedChildren, new { id = key.Result });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id:guid}/children")]
        public async Task<HttpResponseMessage> GetChildren(Guid id,
            [ModelBinder(typeof(PagedQueryModelBinder))] PagedQuery query)
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await GetChildren(intId.Result, query);
        }

        [HttpGet]
        [CustomRoute("{id:int}/descendants/")]
        public async Task<HttpResponseMessage> GetDescendants(int id,
            [ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query)
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaRead))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var items = Services.MediaService.GetPagedDescendants(id, query.Page - 1, query.PageSize, out var total, filter: query.Query);
            var pages = (total + query.PageSize - 1) / query.PageSize;
            var mapped = Mapper.Map<IEnumerable<MediaRepresentation>>(items).ToList();

            // this seems stupid since we usually end up in here by request via guid from the other overload...
            var key = Services.EntityService.GetKeyForId(id, UmbracoObjectTypes.Media);
            if (key.Result == Guid.Empty)
                Request.CreateResponse(HttpStatusCode.NotFound);

            var result = new MediaPagedListRepresentation(mapped, total, pages, query.Page, query.PageSize, LinkTemplates.Media.PagedDescendants, new { id = key.Result });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id:guid}/descendants/")]
        public async Task<HttpResponseMessage> GetDescendants(Guid id,
            [ModelBinder(typeof(PagedQueryModelBinder))] PagedQuery query)
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await GetDescendants(intId.Result, query);
        }

        [HttpGet]
        [CustomRoute("{id:int}/ancestors/")]
        public async Task<HttpResponseMessage> GetAncestors(int id,
            [ModelBinder(typeof(PagedQueryModelBinder))]
            PagedRequest query)
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaRead))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var items = Services.MediaService.GetAncestors(id).ToArray();
            var total = items.Length;
            var pages = (total + query.PageSize - 1) / query.PageSize;
            var paged = items.Skip(ContentControllerHelper.GetSkipSize(query.Page - 1, query.PageSize)).Take(query.PageSize);
            var mapped = Mapper.Map<IEnumerable<MediaRepresentation>>(paged).ToList();

            // this seems stupid since we usually end up in here by request via guid from the other overload...
            var key = Services.EntityService.GetKeyForId(id, UmbracoObjectTypes.Media);
            if (key.Result == Guid.Empty)
                Request.CreateResponse(HttpStatusCode.NotFound);

            var result = new MediaPagedListRepresentation(mapped, total, pages, query.Page, query.PageSize, LinkTemplates.Media.PagedAncestors, new { id = key.Result });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id:guid}/ancestors/")]
        public async Task<HttpResponseMessage> GetAncestors(Guid id,
            [ModelBinder(typeof(PagedQueryModelBinder))] PagedRequest query)
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await GetAncestors(intId.Result, query);
        }

        [HttpGet]
        [CustomRoute("search")]
        public async Task<HttpResponseMessage> Search(
            [ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query)
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, ContentResourceAccess.Empty(), AuthorizationPolicies.MediaRead))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //TODO: Authorize this! how? Same as core, i guess we just filter the results

            if (query.Query.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

            //Query prepping - ensure that we only search for media items...
            var mediaQuery = "__IndexType:media AND " + query.Query;

            //search
            var result = SearchProvider.Search(
                SearchProvider.CreateSearchCriteria().RawQuery(mediaQuery),
                ContentControllerHelper.GetMaxResults(query.Page, query.PageSize));

            //paging
            var paged = result.Skip(ContentControllerHelper.GetSkipSize(query.Page - 1, query.PageSize)).ToArray();
            var pages = (result.TotalItemCount + query.PageSize - 1) / query.PageSize;

            var foundContent = Enumerable.Empty<IMedia>();
            
            //Map to Imedia
            if (paged.Any())
            {
                foundContent = Services.MediaService.GetByIds(paged.Select(x => x.Id)).WhereNotNull();
            }
            
            //Map to representation
            var items = Mapper.Map<IEnumerable<MediaRepresentation>>(foundContent).ToList();

            //return as paged list of media items
            var representation = new MediaPagedListRepresentation(items, result.TotalItemCount, pages, query.Page, query.PageSize, LinkTemplates.Media.Search, new { query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        // Media CRUD:
        
        [HttpPost]
        [CustomRoute("")]
        public async Task<HttpResponseMessage> Post(MediaRepresentation content)
        {
            if (content == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intParentId = Services.EntityService.GetIdForKey(content.ParentId, UmbracoObjectTypes.Media);
            if (!intParentId) return Request.CreateResponse(HttpStatusCode.NotFound);

            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(intParentId.Result), AuthorizationPolicies.MediaCreate))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            try
            {
                //we cannot continue here if the mandatory items are empty (i.e. name, etc...)
                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Media.Root);
                }

                var contentType = Services.ContentTypeService.GetMediaType(content.ContentTypeAlias);
                if (contentType == null)
                {
                    ModelState.AddModelError("content.contentTypeAlias", "No media type found with alias " + content.ContentTypeAlias);
                    throw ValidationException(ModelState, content, LinkTemplates.Media.Root);
                }

                //create an item before persisting of the correct content type
                var created = Services.MediaService.CreateMedia(content.Name, content.ParentId, content.ContentTypeAlias, ClaimsPrincipal.GetUserId() ?? 0);

                //Validate properties
                var validator = new ContentPropertyValidator<IMedia>(ModelState, Services.DataTypeService);
                validator.ValidateItem(content, created);

                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Media.Root);
                }

                Mapper.Map(content, created);
                Services.MediaService.Save(created, ClaimsPrincipal.GetUserId() ?? 0);
                
                var msg = Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<MediaRepresentation>(created));
                AddLocationResponseHeader(msg, LinkTemplates.Media.Self.CreateLink(new { id = created.Id }));                

                return msg;
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpPut]
        [CustomRoute("{id:int}")]
        public async Task<HttpResponseMessage> Put(int id, MediaRepresentation content)
        {
            if (content == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaUpdate))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            try
            {
                var found = Services.MediaService.GetById(id);
                if (found == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                Mapper.Map(content, found);
                Services.MediaService.Save(found, ClaimsPrincipal.GetUserId() ?? 0);

                var rep = Mapper.Map<MediaRepresentation>(found);
                return Request.CreateResponse(HttpStatusCode.OK, rep);
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpPut]
        [CustomRoute("{id:guid}")]
        public async Task<HttpResponseMessage> Put(Guid id, MediaRepresentation content)
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await Put(intId.Result, content);
        }

        [HttpDelete]
        [CustomRoute("{id:int}")]
        public virtual async Task<HttpResponseMessage> Delete(int id)
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaDelete))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var found = Services.MediaService.GetById(id);
            if (found == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            Services.MediaService.Delete(found);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [CustomRoute("{id:guid}")]
        public virtual async Task<HttpResponseMessage> Delete(Guid id)
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await Delete(intId.Result);
        }

        /// <summary>
        /// Update a media item's file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        [HttpPut]
        [CustomRoute("{id:int}/upload")]
        public async Task<HttpResponseMessage> UploadFile(int id, string property = "umbracoFile")
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaUpdate))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "The request doesn't contain valid content!");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.Contents.Count != 1)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This method only works with a single file at a time");

            var file = (StreamContent)provider.Contents.First();
            var name = file.Headers.ContentDisposition.FileName;
            var contentType = file.Headers.ContentType;
            var dataStream = await file.ReadAsStreamAsync();

            //build an in-memory file for umbraco
            var httpFile = new MemoryFile(dataStream, contentType.ToString(), name);

            var media = Services.MediaService.GetById(id);
            if (media == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            media.SetValue(property, httpFile);
            Services.MediaService.Save(media, ClaimsPrincipal.GetUserId() ?? 0);

            return Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<MediaRepresentation>(media));
        }

        /// <summary>
        /// Update a media item's file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        [HttpPut]
        [CustomRoute("{id:guid}/upload")]
        public async Task<HttpResponseMessage> UploadFile(Guid id, string property = "umbracoFile")
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await UploadFile(intId.Result, property);
        }

        /// <summary>
        /// Create a media item by posting a file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediaType"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        [HttpPost]
        [CustomRoute("{id:int}/upload")]
        public async Task<HttpResponseMessage> PostFile(int id, string mediaType = null, string property = Constants.Conventions.Media.File)
        {
            if (!await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, new ContentResourceAccess(id), AuthorizationPolicies.MediaCreate))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "The request doesn't contain valid content!");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.Contents.Count != 1)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This method only works with a single file at a time");

            var file = (StreamContent)provider.Contents.First();
            var name = file.Headers.ContentDisposition.FileName;
            var safeFileName = file.Headers.ContentDisposition.FileName.ToSafeFileName();
            var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

            if (ContentSectionConfig.IsFileAllowedForUpload(ext) == false)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Files of this type not allowed");

            if (string.IsNullOrEmpty(mediaType))
            {
                mediaType = Constants.Conventions.MediaTypes.File;
                if (ContentSectionConfig.ImageFileTypes.Contains(ext))
                {
                    mediaType = Constants.Conventions.MediaTypes.Image;
                }
            }

            var contentType = file.Headers.ContentType;
            var dataStream = await file.ReadAsStreamAsync();

            //build an in-memory file for umbraco
            var httpFile = new MemoryFile(dataStream, contentType.ToString(), name);

            var media = Services.MediaService.CreateMedia(name, id, mediaType);
            media.SetValue(property, httpFile);
            Services.MediaService.Save(media, ClaimsPrincipal.GetUserId() ?? 0);
            return Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<MediaRepresentation>(media));
        }

        /// <summary>
        /// Create a media item by posting a file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mediaType"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        [HttpPost]
        [CustomRoute("{id:guid}/upload")]
        public async Task<HttpResponseMessage> PostFile(Guid id, string mediaType = null, string property = Constants.Conventions.Media.File)
        {
            //We need to do the INT lookup from a GUID since the INT is what governs security there's no way around this right now
            var intId = Services.EntityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (intId.Result < 0)
                Request.CreateResponse(HttpStatusCode.NotFound);
            return await PostFile(intId.Result, mediaType, property);
        }
    }
}
