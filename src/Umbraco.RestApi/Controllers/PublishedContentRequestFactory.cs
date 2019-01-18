using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// Used for custom routed pages that are being integrated with the Umbraco data but are not
    /// part of the umbraco request pipeline. This allows umbraco macros to be able to execute in this scenario.
    /// </summary>
    /// <remarks>
    /// A PR was made for the REST API here: https://github.com/umbraco/UmbracoRestApi/pull/20/files but that didn't quite fit with
    /// recent changes so this was made instead which is a copy of the MVC attribute but ported for webapi which requires explicitly setting
    /// a content id
    /// 
    /// This is inspired from this discussion:
    /// http://our.umbraco.org/forum/developers/extending-umbraco/41367-Umbraco-6-MVC-Custom-MVC-Route?p=3
    /// 
    /// which is based on custom routing found here:
    /// http://shazwazza.com/post/Custom-MVC-routing-in-Umbraco
    /// </remarks>
    public class PublishedContentRequestFactory : IPublishedContentRequestFactory
    {
        private readonly Func<string, IEnumerable<string>> _getRolesForUser;
        private readonly UmbracoContext _umbracoContext;
        private readonly IWebRoutingSection _routingSection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="routingSection"></param>
        /// <param name="getRolesForUser"></param>
        public PublishedContentRequestFactory(
            UmbracoContext umbracoContext,              
            IWebRoutingSection routingSection,
            Func<string, IEnumerable<string>> getRolesForUser)
        {
            _getRolesForUser = getRolesForUser ?? throw new ArgumentNullException(nameof(getRolesForUser));
            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _routingSection = routingSection ?? throw new ArgumentNullException(nameof(routingSection));
        }        

        /// <summary>
        /// Creates and sets the <see cref="PublishedContentRequest"/>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="requestUri"></param>
        public virtual void Create(IPublishedContent content, Uri requestUri)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (requestUri == null) throw new ArgumentNullException(nameof(requestUri));

            _umbracoContext.PublishedContentRequest =
                new PublishedContentRequest(
                    requestUri,
                    _umbracoContext.RoutingContext,
                    _routingSection,
                    _getRolesForUser)
                {
                    PublishedContent = content
                };

            _umbracoContext.PublishedContentRequest.Prepare();
        }        
    }
}