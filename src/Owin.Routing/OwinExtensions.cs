using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Owin.Routing
{
	internal static class OwinExtensions
	{
		private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto
		};

		public static async Task<T> ReadJson<T>(this IOwinContext context)
		{
			var s = await context.ReadStringAsync();
			return JsonConvert.DeserializeObject<T>(s, JsonSerializerSettings);
		}

		public static async Task<JToken> ReadJToken(this IOwinContext context)
		{
			var s = await context.ReadStringAsync();
			return JToken.Parse(s);
		}

		public static async Task<JObject> ReadJObject(this IOwinContext context)
		{
			var s = await context.ReadStringAsync();
			return JObject.Parse(s);
		}

		public static async Task<JArray> ReadJArray(this IOwinContext context)
		{
			var s = await context.ReadStringAsync();
			return JArray.Parse(s);
		}

		public static async Task<T[]> ReadObjectArray<T>(this IOwinContext context)
		{
			var arr = await context.ReadJArray();
			var serializer = JsonSerializer.CreateDefault(JsonSerializerSettings);
			return arr.OfType<JObject>().Select(item => item.ToObject<T>(serializer)).ToArray();
		}

		public static async Task WriteJson(this IOwinContext context, object value)
		{
			// TODO async serialization
			var json = JsonConvert.SerializeObject(value, JsonSerializerSettings);
			context.Response.Headers.Set("Content-Type", "application/json");
			context.Response.Headers.Set("Content-Encoding", "utf8");
			context.Response.ContentType = "application/json";
			var bytes = Encoding.UTF8.GetBytes(json);
			await context.Response.WriteAsync(bytes);
		}

		public static async Task<string> ReadStringAsync(this IOwinContext context)
		{
			var stream = await context.ReadStreamAsync();
			stream.Close();
			return Encoding.UTF8.GetString(stream.ToArray());
		}

		public static async Task<MemoryStream> ReadStreamAsync(this IOwinContext context)
		{
			var stream = new MemoryStream();
			const int size = 8 * 1024;
			var buffer = new byte[size];

			var read = await context.Request.Body.ReadAsync(buffer, 0, size);
			while (read > 0)
			{
				stream.Write(buffer, 0, read);
				buffer = new byte[size];
				read = await context.Request.Body.ReadAsync(buffer, 0, size);
			}

			stream.Flush();
			stream.Position = 0;

			return stream;
		}
	}
}
