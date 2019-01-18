using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    internal class MemberServiceMocks
    {
        internal static void SetupMocksForPost(ServiceContext serviceContext)
        {
            var mockMemberService = Mock.Get(serviceContext.MemberService);
            mockMemberService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedMember());
            mockMemberService.Setup(x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => ModelMocks.SimpleMockedMember(8888));
            mockMemberService.Setup(x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IMemberType>()))
               .Returns(() => ModelMocks.SimpleMockedMember(8888));

            var mockMemberTypeService = Mock.Get(serviceContext.MemberTypeService);
            mockMemberTypeService.Setup(x => x.Get(It.IsAny<string>())).Returns(ModelMocks.SimpleMockedMemberType());

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
