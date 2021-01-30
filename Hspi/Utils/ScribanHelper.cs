using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

#nullable enable

namespace Hspi.Utils
{
    internal static class ScribanHelper
    {
        public static IDictionary<string, object> ConvertToStringObjectDictionary(IDictionary<string, string> source)
        {
            var destination = new Dictionary<string, object>();
            foreach (var pair in source)
            {
                destination.Add(pair.Key, pair.Value);
            }
            return destination;
        }

        public static T FromDictionary<T>(IDictionary<string, string> source) where T : class
        {
            var json = JsonConvert.SerializeObject(source, Formatting.None);
            return Deserialize<T>(json);
        }

        public static T FromDictionary<T>(IDictionary<string, object> source) where T : class
        {
            var json = JsonConvert.SerializeObject(source, Formatting.None);
            return Deserialize<T>(json);
        }

        private static T Deserialize<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new BoolConverter());
            var obj = serializer.Deserialize<T>(new JsonTextReader(new StringReader(json)));
            return obj;
        }

        public static IDictionary<string, object> ToDictionary<T>(T obj)
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new LowercaseContractResolver();
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return dict;
        }

        private class LowercaseContractResolver : DefaultContractResolver
        {
#pragma warning disable CA1308 // Normalize strings to uppercase

            protected override string ResolvePropertyName(string propertyName) => propertyName.ToLowerInvariant();

#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        private class BoolConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((bool)value) ? "on" : "off");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                return reader?.Value?.ToString() == "on";
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(bool);
            }
        }
    }
}