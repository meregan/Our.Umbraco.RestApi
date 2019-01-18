using AutoMapper;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Profiling;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Models.Mapping;
using Umbraco.RestApi.Tests.TestHelpers;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.RestApi.Tests
{
    [TestFixture]
    public class AutoMapperTests
    {
        [Test]
        public void Assert_Valid_Mappings()
        {
            //new app context
            var appCtx = ApplicationContext.EnsureContext(
                ServiceMocks.GetDatabaseContext(),
                ServiceMocks.GetServiceContext(),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()),
                true);

            Mapper.Initialize(configuration =>
            {
                var mappers = new MapperConfiguration[]
                {
                    new ContentModelMapper(),
                    new MediaModelMapper(),
                    new MemberModelMapper(),
                    new PublishedContentMapper(),
                    new RelationModelMapper()
                };
                foreach (var mapper in mappers)
                {
                    mapper.ConfigureMappings(configuration, appCtx);
                }
            });

            //TODO: get this testing our mappings
            Mapper.AssertConfigurationIsValid();
        }
    }
}