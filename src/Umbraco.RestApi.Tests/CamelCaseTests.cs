using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Tests
{
    [TestFixture]
    public class CamelCaseTests
    {
        [Test]
        public void Property_Aliases_Not_Changed()
        {
            var obj = new ContentRepresentation()
            {
                Properties = new Dictionary<string, object>
                {
                    {"Property1", "value 1"},
                    {"property 2","value 2"}
                }
            };
            
            var serialize = JsonConvert.SerializeObject(obj);

            var deserialize = JsonConvert.DeserializeObject<ContentRepresentation>(serialize);

            Assert.IsTrue(deserialize.Properties.Keys.Contains("Property1"));
            Assert.IsTrue(deserialize.Properties.Keys.Contains("property 2"));
        }
    }
}