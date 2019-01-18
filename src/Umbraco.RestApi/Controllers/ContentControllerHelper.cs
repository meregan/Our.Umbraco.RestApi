using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using Umbraco.Core.Services;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    public class ContentControllerHelper
    {
        private readonly ILocalizedTextService _textService;

        public ContentControllerHelper(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        /// <summary>
        /// Helper method to get total amount of pages.
        /// </summary>
        /// <param name="totalRecords">Total number of results.</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <returns></returns>
        internal static int GetTotalPages(long totalRecords, int pageSize)
        {
            var totalPages = ((int)totalRecords + pageSize - 1) / pageSize;
            return totalPages;
        }

        /// <summary>
        /// Helper method to get the number of results to skip.
        /// </summary>
        /// <param name="pageIndex">Index of the current page (0-based).</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <returns></returns>
        internal static int GetSkipSize(long pageIndex, int pageSize)
        {
            if (pageIndex >= 0 && pageSize > 0)
            {
                return Convert.ToInt32(pageIndex * pageSize);
            }
            return 0;
        }

        /// <summary>
        /// Helper method to determine max number of results to use in an Examine query.
        /// Examine does not support take - only skip. Use this to limit the query to a maximum of what we need
        /// and then use skip to get rid of the items we do not need, leading up to this paging result.
        /// https://shazwazza.com/post/paging-with-examine/
        /// </summary>
        /// <param name="page">Number of the current page.</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <returns></returns>
        /// <remarks>First page is page 1 (not 0-based).</remarks>
        internal static int GetMaxResults(long page, int pageSize)
        {
            return Convert.ToInt32(page * pageSize);
        }

        internal IDictionary<string, ContentPropertyInfo> GetDefaultFieldMetaData(ClaimsPrincipal user)
        {
            var cultureClaim = user.FindFirst(ClaimTypes.Locality);
            if (cultureClaim == null)
                throw new InvalidOperationException($"The required user claim {ClaimTypes.Locality} is missing");

            var userCulture = new CultureInfo(cultureClaim.Value);
            //TODO: This shouldn't actually localize based on the current user!!!
            // this should localize based on the current request's Accept-Language and Content-Language headers

            return new Dictionary<string, ContentPropertyInfo>
            {
                {"id", new ContentPropertyInfo{Label = "Id", ValidationRequired = true}},
                {"key", new ContentPropertyInfo{Label = "Key", ValidationRequired = true}},
                {"contentTypeAlias", new ContentPropertyInfo{Label = _textService.Localize("content/documentType", userCulture), ValidationRequired = true}},
                {"parentId", new ContentPropertyInfo{Label = "Parent Id", ValidationRequired = true}},
                {"hasChildren", new ContentPropertyInfo{Label = "Has Children"}},
                {"templateId", new ContentPropertyInfo{Label = _textService.Localize("template/template", userCulture) + " Id", ValidationRequired = true}},
                {"sortOrder", new ContentPropertyInfo{Label = _textService.Localize("general/sort", userCulture)}},
                {"name", new ContentPropertyInfo{Label = _textService.Localize("general/name", userCulture), ValidationRequired = true}},
                {"urlName", new ContentPropertyInfo{Label = _textService.Localize("general/url", userCulture) + " " + _textService.Localize("general/name", userCulture)}},
                {"writerName", new ContentPropertyInfo{Label = _textService.Localize("content/updatedBy", userCulture)}},
                {"creatorName", new ContentPropertyInfo{Label = _textService.Localize("content/createBy", userCulture)}},
                {"writerId", new ContentPropertyInfo{Label = "Writer Id"}},
                {"creatorId", new ContentPropertyInfo{Label = "Creator Id"}},
                {"path", new ContentPropertyInfo{Label = _textService.Localize("general/path", userCulture)}},
                {"createDate", new ContentPropertyInfo{Label = _textService.Localize("content/createDate", userCulture)}},
                {"updateDate", new ContentPropertyInfo{Label = _textService.Localize("content/updateDate", userCulture)}},
                {"level", new ContentPropertyInfo{Label = "Level"}},
                {"url", new ContentPropertyInfo{Label = _textService.Localize("general/url", userCulture)}},
                //TODO: Do we use this?
                {"ItemType", new ContentPropertyInfo{Label = _textService.Localize("general/type", userCulture)}}
            };
        }
    }
}
