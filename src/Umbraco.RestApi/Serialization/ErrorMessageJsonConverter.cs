//using System;
//using System.Collections.Generic;
//using Newtonsoft.Json;
//using Umbraco.Core;
//using Umbraco.Web.Rest.Models;

//namespace Umbraco.Web.Rest.Serialization
//{
//    public class ErrorMessageJsonConverter : JsonConverter
//    {
//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool CanConvert(Type objectType)
//        {
//            return objectType == typeof(IEnumerable<ValidationErrorRepresentation>);
//        }

//        public override bool CanRead
//        {
//            get { return false; }
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            writer.WriteStartArray();

//            foreach (var error in (IEnumerable<ValidationErrorRepresentation>)value)
//            {
//                if (error.Field.IsNullOrWhiteSpace())
//                {
//                    serializer.Serialize(writer, error.Message);
//                }
//                else
//                {
//                    serializer.Serialize(writer, error);
//                }
//            }

//            writer.WriteEndArray();
//        }

//    }
//}