using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.RestApi.Serialization
{
    /// <summary>
    /// Custom converter to ensure that key for the dictionary doesn't get camelcased
    /// </summary>
    public class ExplicitlyCasedDictionaryKeyJsonConverter<TVal> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IDictionary<string, TVal>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<IDictionary<string, TVal>>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var property in (IDictionary<string, TVal>)value)
            {
                writer.WritePropertyName(property.Key);
                serializer.Serialize(writer, property.Value);
            }

            writer.WriteEndObject();
        }
    }
}