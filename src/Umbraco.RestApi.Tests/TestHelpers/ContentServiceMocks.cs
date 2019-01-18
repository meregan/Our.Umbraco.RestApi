using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    internal class ContentServiceMocks
    {
        internal static void SetupMocksForPost(ServiceContext serviceContext)
        {
            var mockContentService = Mock.Get(serviceContext.ContentService);
            mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());
            mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));
            mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);
            mockContentService.Setup(x => x.CreateContent(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() => ModelMocks.SimpleMockedContent(8888));

            var entityServiceMock = Mock.Get(serviceContext.EntityService);
            entityServiceMock.Setup(x => x.GetIdForKey(456.ToGuid(), UmbracoObjectTypes.Document)).Returns(Attempt.Succeed(456));

            var fileServiceMock = Mock.Get(serviceContext.FileService);
            fileServiceMock.Setup(x => x.GetTemplate(9.ToGuid())).Returns(
                Mock.Of<ITemplate>(template => 
                template.Id == 9
                && template.Key == 9.ToGuid()
                && template.Name == "test" 
                && template.IsMasterTemplate == false));

            var mockContentTypeService = Mock.Get(serviceContext.ContentTypeService);
            mockContentTypeService.Setup(x => x.GetContentType(It.IsAny<string>())).Returns(ModelMocks.SimpleMockedContentType());

            var mockDataTypeService = Mock.Get(serviceContext.DataTypeService);
            mockDataTypeService.Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>())).Returns(new PreValueCollection(Enumerable.Empty<PreValue>()));

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(new ModelMocks.SimplePropertyEditor());

            Func<IEnumerable<Type>> producerList = Enumerable.Empty<Type>;
            var mockPropertyEditorResolver = new Mock<PropertyEditorResolver>(
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger>(),
                producerList,
                Mock.Of< IRuntimeCacheProvider>());

            mockPropertyEditorResolver.Setup(x => x.PropertyEditors).Returns(new[] { new ModelMocks.SimplePropertyEditor() });

            PropertyEditorResolver.Current = mockPropertyEditorResolver.Object;
        }
    }
}
