using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Tests.TestHelpers;
using Umbraco.Core.Models.EntityBase;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.RestApi.Tests
{    
    [TestFixture]
    public class RelationsControllerTests : ControllerTests
    {
        [Test]
        public async Task Get_Root_With_OPTIONS()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {
                    var mockRelationService = Mock.Get(testServices.ServiceContext.RelationService);
                });

            await Get_Root_With_OPTIONS(startup.UseDefaultTestSetup, RouteConstants.RelationsSegment);
        }

        [Test]
        public async Task Get_Root_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {
                    var mockRelationService = Mock.Get(testServices.ServiceContext.RelationService);
                    mockRelationService.Setup(x => x.GetAllRelationTypes(It.IsAny<int[]>()))
                        .Returns(new[]
                        {
                            new RelationType(Constants.ObjectTypes.DocumentGuid, Constants.ObjectTypes.DocumentGuid, "test1", "Test1"),
                            new RelationType(Constants.ObjectTypes.MediaGuid, Constants.ObjectTypes.MediaGuid, "test2", "Test2"),
                        });
                });

            var djson = await Get_Root_Result(startup.UseDefaultTestSetup, RouteConstants.RelationsSegment);
            Assert.AreEqual(2, djson["_links"]["relationtype"].Count());
            Assert.AreEqual(2, djson["_embedded"]["relationtype"].Count());
        }

        [Test]
        public async Task Get_Id_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    var mockRelationService = Mock.Get(testServices.ServiceContext.RelationService);
                    mockRelationService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedRelation(123, 4567, 8910, ModelMocks.SimpleMockedRelationType()));
                });

            var djson = await Get_Id_Result(startup.UseDefaultTestSetup, RouteConstants.RelationsSegment);
            Assert.AreEqual("/umbraco/rest/v1/relations/123", djson["_links"]["self"]["href"].Value<string>());
            Assert.AreEqual("/umbraco/rest/v1/relations", djson["_links"]["root"]["href"].Value<string>());
            Assert.AreEqual("/umbraco/rest/v1/relations/relationtype/testType", djson["_links"]["relationtype"]["href"].Value<string>());
            Assert.AreEqual("/umbraco/rest/v1/content/published/{id}", djson["_links"]["content"]["href"].Value<string>());
        }

        [Test]
        public async Task Post_Is_201_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    RelationServiceMocks.SetupMocksForPost(testServices.ServiceContext);
                });

            await base.Post_Is_201_Response(startup.UseDefaultTestSetup, RouteConstants.RelationsSegment, new StringContent(@"{
  ""relationTypeAlias"": ""testType"",
  ""parentId"": """ + 8910.ToGuid() + @""",
  ""childId"" : """ + 567.ToGuid() + @""",
  ""comment"" : ""Comment""
}", Encoding.UTF8, "application/json"));
        }
        
        [Test]
        public async Task Post_Is_400_Validation_Required_Fields()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    RelationServiceMocks.SetupMocksForPost(testServices.ServiceContext);
                });

            using (var server = TestServer.Create(builder => startup.UseDefaultTestSetup(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{RouteConstants.RelationsSegment}"),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                //NOTE: it is missing parent id
                request.Content = new StringContent(@"{
  ""relationTypeAlias"": """",
  ""childId"" : """ + 1234.ToGuid() + @""",
  ""comment"" : ""Comment""
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual(2, djson["totalResults"].Value<int>());
                Assert.AreEqual("relation.ParentId", djson["_embedded"]["errors"][0]["logRef"].Value<string>());
                Assert.AreEqual("relation.RelationTypeAlias", djson["_embedded"]["errors"][1]["logRef"].Value<string>());                
            }
        }

        [Test]
        public async Task Put_Is_200_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    RelationServiceMocks.SetupMocksForPost(testServices.ServiceContext);
                });

            await base.Put_Is_200_Response(startup.UseDefaultTestSetup, RouteConstants.RelationsSegment, new StringContent(@"{
  ""relationTypeAlias"": ""testType"",
  ""parentId"": 1235,
  ""childId"" : 1234,
  ""comment"" : ""New comment""
}", Encoding.UTF8, "application/json"));

        }
        
    }

}