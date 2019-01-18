using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Security;
using Examine.Providers;
using LightInject;
using Moq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Profiling;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.RestApi.Controllers;
using Umbraco.RestApi.Security;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    public abstract class TestControllerActivatorBase : DefaultHttpControllerActivator, IHttpControllerActivator
    {
        public ApplicationContext ApplicationContext { get; }

        protected TestControllerActivatorBase(ApplicationContext applicationContext)
        {
            ApplicationContext = applicationContext;
        }

        IHttpController IHttpControllerActivator.Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            if (typeof(UmbracoApiControllerBase).IsAssignableFrom(controllerType))
            {
                var owinContext = request.GetOwinContext();

                var mockedTypedContentQuery = Mock.Of<ITypedPublishedContentQuery>();
                
                //httpcontext with an auth'd user
                var httpContext = Mock.Of<HttpContextBase>(http => http.User == owinContext.Authentication.User);
                //chuck it into the props since this is what MS does when hosted
                request.Properties["MS_HttpContext"] = httpContext;

                var webSecurity = new Mock<WebSecurity>(null, null);

                //mock CurrentUser
                var admin = Mock.Of<IUser>(u => u.IsApproved == true
                                                && u.IsLockedOut == false
                                                && u.AllowedSections == owinContext.Authentication.User.GetAllowedSections()
                                                //&& u.Email == "admin@admin.com"
                                                && u.Id == owinContext.Authentication.User.GetUserId().Value
                                                && u.Language == owinContext.Authentication.User.GetUserCulture().DisplayName
                                                && u.Name == owinContext.Authentication.User.GetUserName()
                                                && u.StartContentIds == owinContext.Authentication.User.GetContentStartNodeIds()
                                                && u.StartMediaIds == owinContext.Authentication.User.GetMediaStartNodeIds()
                                                && u.Username == owinContext.Authentication.User.GetLoginName());

                webSecurity.Setup(x => x.CurrentUser).Returns(admin);
                var mockedUserService = Mock.Get(ApplicationContext.Services.UserService);
                mockedUserService.Setup(x => x.GetUserById(admin.Id)).Returns(admin);
                
                var umbCtx = UmbracoContext.EnsureContext(
                    //set the user of the HttpContext
                    httpContext,
                    ApplicationContext,
                    webSecurity.Object,
                    Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == UrlProviderMode.Auto.ToString())),
                    Enumerable.Empty<IUrlProvider>(),
                    true); //replace it

                var urlHelper = new Mock<IUrlProvider>();
                urlHelper.Setup(provider => provider.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<int>(), It.IsAny<Uri>(), It.IsAny<UrlProviderMode>()))
                    .Returns("/hello/world/1234");

                var membershipHelper = new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>());

                var umbHelper = new UmbracoHelper(umbCtx,
                    Mock.Of<IPublishedContent>(),
                    mockedTypedContentQuery,
                    Mock.Of<IDynamicPublishedContentQuery>(),
                    Mock.Of<ITagQuery>(),
                    Mock.Of<IDataTypeService>(),
                    new UrlProvider(umbCtx, new[]
                    {
                        urlHelper.Object
                    }, UrlProviderMode.Auto),
                    Mock.Of<ICultureDictionary>(),
                    Mock.Of<IUmbracoComponentRenderer>(),
                    membershipHelper);

                var searchProvider = Mock.Of<BaseSearchProvider>();

                var mockSettings = MockUmbracoSettings.GenerateMockSettings();

                //build a container which will be used to construct the controllers
                var container = new ServiceContainer();
                container.Register<UmbracoHelper>(factory => umbHelper);
                container.Register<UmbracoContext>(factory => umbHelper.UmbracoContext);
                container.Register<BaseSearchProvider>(factory => searchProvider);
                container.Register<IUmbracoSettingsSection>(factory => mockSettings);
                container.Register<IContentSection>(factory => mockSettings.Content);
                container.Register<IPublishedContentRequestFactory>(factory => Mock.Of<IPublishedContentRequestFactory>());
                container.Register(controllerType);

                var testServices = new TestServices(request, umbHelper.UmbracoContext, mockedTypedContentQuery, ApplicationContext.Services, searchProvider, mockSettings);

                return CreateController(container, controllerType, umbHelper, testServices);
            }
            //default
            return base.Create(request, controllerDescriptor, controllerType);
        }

        protected abstract ApiController CreateController(
            IServiceFactory container, Type controllerType, 
            UmbracoHelper helper,
            TestServices testServices);
    }
}