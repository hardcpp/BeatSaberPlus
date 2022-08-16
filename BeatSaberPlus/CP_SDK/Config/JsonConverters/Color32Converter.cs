using Newtonsoft.Json;
using System;
using UnityEngine;

namespace CP_SDK.Config.JsonConverters
{
    public class Color32Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Color32))
            {
                return true;
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var t = serializer.Deserialize(reader);
            var iv = JsonConvert.DeserializeObject<Color32>(t.ToString());
            return iv;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Color32 v = (Color32)value;

            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(v.r);
            writer.WritePropertyName("g");
            writer.WriteValue(v.g);
            writer.WritePropertyName("b");
            writer.WriteValue(v.b);
            writer.WritePropertyName("a");
            writer.WriteValue(v.a);
            writer.WriteEndObject();
        }
    }
}
