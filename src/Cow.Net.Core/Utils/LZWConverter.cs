﻿using System;
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
            if (reader.Value == null)
                return null;

            if (reader.TokenType == JsonToken.Null)
            {
                if (!IsNullable(objectType))
                    return null;

                return null;
            }

            string data;

            if (reader.TokenType == JsonToken.String)
            {
                data = reader.Value.ToString().DecompressLZW();
            }
            else
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject(data, objectType);
            }
            catch (Exception)
            {
                return null;
            }
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
