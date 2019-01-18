using Moq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    public class ServiceMocks
    {
        public static DatabaseContext GetDatabaseContext()
        {
            //new app context
            var dbCtx = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory2>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test");
            //ensure these are set so that the appctx is 'Configured'
            dbCtx.Setup(x => x.CanConnect).Returns(true);
            dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(true);
            return dbCtx.Object;
        }

        public static ServiceContext GetServiceContext()
        {
            //Create mocked services that we are going to pass to the callback for unit tests to modify
            // before passing these services to the main container objects            
            var mockedContentService = Mock.Of<IContentService>();
            var mockedContentTypeService = Mock.Of<IContentTypeService>();
            var mockedMemberTypeService = Mock.Of<IMemberTypeService>();
            var mockedMediaService = Mock.Of<IMediaService>();
            var mockedMemberService = Mock.Of<IMemberService>();
            var mockedTextService = Mock.Of<ILocalizedTextService>();
            var mockedDataTypeService = Mock.Of<IDataTypeService>();
            var mockedRelationService = Mock.Of<IRelationService>();
            var mockedMigrationService = new Mock<IMigrationEntryService>();
            var mockedUserService = new Mock<IUserService>();
            var mockedEntityService = new Mock<IEntityService>();
            var mockedFileService = new Mock<IFileService>();

            var serviceContext = new ServiceContext(
                dataTypeService: mockedDataTypeService,
                contentTypeService: mockedContentTypeService,
                contentService: mockedContentService,
                mediaService: mockedMediaService,
                memberService: mockedMemberService,
                localizedTextService: mockedTextService,
                memberTypeService: mockedMemberTypeService,
                relationService: mockedRelationService,
                migrationEntryService: mockedMigrationService.Object,
                userService: mockedUserService.Object,
                entityService: mockedEntityService.Object,
                fileService: mockedFileService.Object);

            return serviceContext;
        }
    }
}