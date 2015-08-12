using System;
using System.Linq;
using System.Reflection;
using Cow.Net.Core.Extensions;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.Utils
{
    public class LZWConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var jsonString = JsonConvert.SerializeObject(value);
            var data = jsonString.CompressToLZW();
            writer.WriteValue(data);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (!IsNullable(objectType))
                    throw new Exception(string.Format("Cannot convert null value to {0}.", objectType));

                return null;
            }

            string data;

            if (reader.TokenType == JsonToken.String)
            {
                data = reader.Value.ToString().DecompressLZW();
            }
            else
            {
                throw new Exception(string.Format("Unexpected token parsing binary. Expected String or StartArray, got {0}.", reader.TokenType));
            }

            var payload = JsonConvert.DeserializeObject(data, objectType);

            return payload;
        }

        public static T Cast<T>(object o)
        {
            return (T)o;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IPayload));
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public static bool IsNullable(Type type)
        {
            return type != null && (!type.GetTypeInfo().IsValueType || IsNullableType(type));
        }

        private static bool IsNullableType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
