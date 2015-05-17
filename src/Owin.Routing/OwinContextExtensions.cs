using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

	/// <summary>
	/// <see cref="IOwinContext"/> extensions.
	/// </summary>
	public static class OwinContextExtensions
	{
		/// <summary>
		/// Gets value of route parameter.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context">The OWIN context.</param>
		/// <param name="name">The parameter name.</param>
		public static T GetRouteValue<T>(this IOwinContext context, string name)
		{
			if (context == null) throw new ArgumentNullException("context");

			var data = context.Get<RouteData>(Keys.RouteData);
			if (data == null) throw new InvalidOperationException();

			var val = data.Values[name];
			return val.ToType<T>();
		}

		/// <summary>
		/// Gets value of route parameter.
		/// </summary>
		/// <param name="context">The OWIN environment.</param>
		/// <param name="name">The parameter name.</param>
		public static string GetRouteValue(this IOwinContext context, string name)
		{
			return context.GetRouteValue<string>(name);
		}

		internal static HttpContextBase HttpContext(this IOwinContext ctx)
		{
			if (ctx == null) throw new ArgumentNullException("ctx");

			var value = ctx.Get<HttpContextBase>(Keys.HttpContext);
			if (value == null)
			{
				value = new HttpContextImpl(ctx);
				ctx.Set(Keys.HttpContext, value);
			}
			return value;
		}

		public static byte[] RequestBytes(this IOwinContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			var value = context.Get<byte[]>(Keys.RequestBytes);
			if (value == null)
			{
				value = context.Request.Body.ToByteArray();
				context.Set(Keys.RequestBytes, value);
			}
			return value;
		}

		/// <summary>
		/// Gets request body parsed as JSON.
		/// </summary>
		/// <param name="context">The OWIN environment.</param>
		public static JToken JsonBody(this IOwinContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			var value = context.Get<JToken>(Keys.JsonBody);
			if (value == null)
			{
				value = context.ReadJToken();
				context.Set(Keys.JsonBody, value);
			}
			return value;
		}

		#region JSON Extensions

		

		/// <summary>
		/// Reads JSON string and materializes it to given type.
		/// </summary>
		/// <typeparam name="T">Type to materialize.</typeparam>
		/// <param name="context">The OWIN context.</param>
		/// <returns>Materialized instance of T.</returns>
		public static T ReadJson<T>(this IOwinContext context)
		{
			using (var reader = Json.CreateReader(context))
			{
				return Json.CreateSerializer().Deserialize<T>(reader);
			}
		}

		/// <summary>
		/// Reads JSON string and parse it to <see cref="JToken"/>.
		/// </summary>
		/// <param name="context">The OWIN context.</param>
		public static JToken ReadJToken(this IOwinContext context)
		{
			using (var reader = Json.CreateReader(context))
			{
				return JToken.Load(reader);
			}
		}

		/// <summary>
		/// Reads JSON string and parse it to <see cref="JObject"/>.
		/// </summary>
		/// <param name="context">The OWIN context.</param>
		public static JObject ReadJObject(this IOwinContext context)
		{
			using (var reader = Json.CreateReader(context))
			{
				return JObject.Load(reader);
			}
		}

		/// <summary>
		/// Reads JSON string and parse it to <see cref="JArray"/>.
		/// </summary>
		/// <param name="context">The OWIN context.</param>
		public static JArray ReadJArray(this IOwinContext context)
		{
			using (var reader = Json.CreateReader(context))
			{
				return JArray.Load(reader);
			}
		}

		/// <summary>
		/// Reads JSON array with mapping to given type.
		/// </summary>
		/// <typeparam name="T">Type of array element.</typeparam>
		/// <param name="context">The OWIN environment.</param>
		public static T[] ReadObjectArray<T>(this IOwinContext context)
		{
			var arr = context.ReadJArray();
			var serializer = Json.CreateSerializer();
			return arr.OfType<JObject>().Select(item => item.ToObject<T>(serializer)).ToArray();
		}

		/// <summary>
		/// Writes given value as JSON string.
		/// </summary>
		/// <param name="context">The OWIN context.</param>
		/// <param name="value">The value to serialize.</param>
		/// <param name="serializerSettings">The serializer settings.</param>
		public static async Task WriteJson(this IOwinContext context, object value, JsonSerializerSettings serializerSettings = null)
		{
			var json = JsonConvert.SerializeObject(value, serializerSettings ?? Json.Settings);
			const string contentType = "application/json";
			context.Response.Headers.Set("Content-Type", contentType);
			context.Response.Headers.Set("Content-Encoding", "utf8");
			context.Response.ContentType = contentType;
			var bytes = Encoding.UTF8.GetBytes(json);
			await context.Response.WriteAsync(bytes);
		}

		#endregion
	}
}
