using System.Net.Http;
using Examine.Providers;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    /// <summary>
    /// A collection of services that tests can use that can be mutated prior to running the test
    /// </summary>
    public class TestServices
    {
        public HttpRequestMessage HttpRequestMessage { get; }
        public UmbracoContext UmbracoContext { get; }
        public ITypedPublishedContentQuery PublishedContentQuery { get; }
        public ServiceContext ServiceContext { get; }
        public BaseSearchProvider SearchProvider { get; }
        public IUmbracoSettingsSection UmbracoSettings { get; }

        public TestServices(HttpRequestMessage httpRequestMessage, UmbracoContext umbracoContext, ITypedPublishedContentQuery publishedContentQuery, ServiceContext serviceContext, BaseSearchProvider searchProvider, IUmbracoSettingsSection umbracoSettings)
        {
            HttpRequestMessage = httpRequestMessage;
            UmbracoContext = umbracoContext;
            PublishedContentQuery = publishedContentQuery;
            ServiceContext = serviceContext;
            SearchProvider = searchProvider;
            UmbracoSettings = umbracoSettings;
        }
    }
}