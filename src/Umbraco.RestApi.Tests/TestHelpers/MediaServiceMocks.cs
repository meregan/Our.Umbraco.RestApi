using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    internal class MediaServiceMocks
    {
        internal static void SetupMocksForPost(ServiceContext serviceContext)
        {
            var mockMediaService = Mock.Get(serviceContext.MediaService);
            mockMediaService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedMedia());
            mockMediaService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IMedia>(new[] { ModelMocks.SimpleMockedMedia(789) }));
            mockMediaService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);
            mockMediaService.Setup(x => x.CreateMedia(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() => ModelMocks.SimpleMockedMedia(8888));

            var entityServiceMock = Mock.Get(serviceContext.EntityService);
            entityServiceMock.Setup(x => x.GetIdForKey(456.ToGuid(), UmbracoObjectTypes.Media)).Returns(Attempt.Succeed(456));

            var mockContentTypeService = Mock.Get(serviceContext.ContentTypeService);
            mockContentTypeService.Setup(x => x.GetMediaType(It.IsAny<string>())).Returns(ModelMocks.SimpleMockedMediaType());

            var mockDataTypeService = Mock.Get(serviceContext.DataTypeService);
            mockDataTypeService.Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>())).Returns(new PreValueCollection(Enumerable.Empty<PreValue>()));

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(new ModelMocks.SimplePropertyEditor());

            Func<IEnumerable<Type>> producerList = Enumerable.Empty<Type>;
            var mockPropertyEditorResolver = new Mock<PropertyEditorResolver>(
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger>(),
                producerList,
                Mock.Of<IRuntimeCacheProvider>());

            mockPropertyEditorResolver.Setup(x => x.PropertyEditors).Returns(new[] { new ModelMocks.SimplePropertyEditor() });

            PropertyEditorResolver.Current = mockPropertyEditorResolver.Object;
        }
    }
}