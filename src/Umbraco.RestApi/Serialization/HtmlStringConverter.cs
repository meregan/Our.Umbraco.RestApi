using System;
using System.Web;
using Newtonsoft.Json;

namespace Umbraco.RestApi.Serialization
{
    /// <summary>
    /// Custom converter for IHtmlString since that does not serialize to json properly
    /// </summary>
    public class HtmlStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
                serializer.Serialize(writer, value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IHtmlString).IsAssignableFrom(objectType);
        }
    }
}