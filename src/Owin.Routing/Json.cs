using System;
using System.IO;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Owin.Routing
{
	internal static class Json
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto
		};

		public static JsonReader CreateReader(IOwinContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			return new JsonTextReader(new StreamReader(context.Request.Body));
		}

		public static JsonSerializer CreateSerializer()
		{
			return JsonSerializer.CreateDefault(Settings);
		}
	}

	internal class CustomIntJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (objectType == typeof(int?) && reader.Value == null)
				return null;
			return int.Parse(reader.Value.ToString());
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(int?) || objectType == typeof(int);
		}
	}
}
