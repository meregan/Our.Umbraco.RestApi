using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.RestApi.Serialization;
using WebApi.Hal;

namespace Umbraco.RestApi.Controllers
{
    public class HalFormatterConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Insert(0, new XmlHalMediaTypeFormatter());
            var jsonFormatter = new JsonHalMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()                    
                }
            };
            jsonFormatter.SerializerSettings.Converters.Add(new HtmlStringConverter());
            controllerSettings.Formatters.Insert(0, jsonFormatter);
        }
    }
}