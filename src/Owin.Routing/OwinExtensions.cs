using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Owin.Routing
{
	public static class OwinExtensions
	{
		private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto
		};

		/// <summary>
		/// Reads JSON string and materializes it to given type.
		/// </summary>
		/// <typeparam name="T">Type to materialize.</typeparam>
		/// <param name="context">The OWIN context.</param>
		/// <returns>Materialized instance of T.</returns>
		public static async Task<T> ReadJson<T>(this IOwinContext context)
		{
			var s = await context.ReadStringAsync();
			return JsonConvert.DeserializeObject<T>(s, JsonSerializerSettings);
		}

		public static async Task<T[]> ReadObjectArray<T>(this IOwinContext context)
		{
			var arr = await context.ReadJArray();
			var serializer = JsonSerializer.CreateDefault(JsonSerializerSettings);
			return arr.OfType<JObject>().Select(item => item.ToObject<T>(serializer)).ToArray();
		}

		/// <summary>
		/// Reads JSON string and parse it to <see cref="JToken"/>.
		/// </summary>
		/// <param name="context">The OWIN context.</param>
		public static async Task<JToken> ReadJToken(this IOwinContext context)
		{
			var s = await context.ReadStringAsync();
			return JToken.Parse(s);
		}

		/// <summary>
		/// Reads JSON string and parse it to <see cref="JObject"/>.
		/// </summary>
		/// <param name="context">The OWIN context.</param>
		public static async Task<JObject> ReadJObject(this IOwinContext context)
		{
			var s = await context.ReadStringAsync();
			return JObject.Parse(s);
		}

		/// <summary>
		/// Reads JSON string and parse it to <see cref="JArray"/>.
		/// </summary>
		/// <param name="context">The OWIN context.</param>
		public static async Task<JArray> ReadJArray(this IOwinContext context)
		{
			var s = await context.ReadStringAsync();
			return JArray.Parse(s);
		}

		/// <summary>
		/// Writes given value as JSON string.
		/// </summary>
		/// <param name="context">The OWIN context.</param>
		/// <param name="value">The value to serialize.</param>
		public static async Task WriteJson(this IOwinContext context, object value)
		{
			var json = JsonConvert.SerializeObject(value, JsonSerializerSettings);
			const string contentType = "application/json";
			context.Response.Headers.Set("Content-Type", contentType);
			context.Response.Headers.Set("Content-Encoding", "utf8");
			context.Response.ContentType = contentType;
			var bytes = Encoding.UTF8.GetBytes(json);
			await context.Response.WriteAsync(bytes);
		}

		/// <summary>
		/// Reads string from request body.
		/// </summary>
		/// <param name="context">The OWIN request context.</param>
		public static async Task<string> ReadStringAsync(this IOwinContext context)
		{
			var stream = await context.ReadStreamAsync();
			stream.Close();
			return Encoding.UTF8.GetString(stream.ToArray());
		}

		/// <summary>
		/// Reads <see cref="MemoryStream"/> from request body.
		/// </summary>
		/// <param name="context">The OWIN request context.</param>
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
