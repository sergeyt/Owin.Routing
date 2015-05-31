using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;

namespace Owin.Routing
{
	/// <summary>
	/// Builds function to resolve method arguments from <see cref="IOwinContext"/>.
	/// </summary>
	internal static class ParameterMapper
	{
		/// <summary>
		/// Builds function to resolve method arguments from <see cref="IOwinContext"/>.
		/// </summary>
		public static Func<IOwinContext, object[]> Build(MethodBase method)
		{
			if (method == null) throw new ArgumentNullException("method");

			var mappers = (from p in method.GetParameters() select Build(p)).ToArray();
			return ctx => mappers.Select(f => f(ctx)).ToArray();
		}

		public static Func<IOwinContext, object> Build(ParameterInfo parameter)
		{
			var type = parameter.ParameterType;
			if (type == typeof(IOwinContext))
			{
				return ctx => ctx;
			}
			if (type == typeof(IOwinRequest))
			{
				return ctx => ctx.Request;
			}
			if (type == typeof(IOwinResponse))
			{
				return ctx => ctx.Response;
			}
			if (type == typeof(Stream))
			{
				return ctx => ctx.Request.Body;
			}
			if (type == typeof(byte[]))
			{
				return ctx => ctx.RequestBytes();
			}
			if (type == typeof(JToken))
			{
				return ctx => ctx.JsonBody();
			}
			if (type == typeof(JObject))
			{
				return ctx => ctx.JsonBody() as JObject;
			}
			if (type == typeof(JArray))
			{
				return ctx => ctx.JsonBody() as JArray;
			}

			if (parameter.HasAttribute<MapJsonAttribute>())
			{
				return ctx => ctx.JsonBody().ToObject(type, Json.CreateSerializer());
			}

			var mapAttribute = parameter.GetAttribute<MapAttribute>();
			if (null != mapAttribute)
			{
				return ctx => MapParameter(ctx, mapAttribute.Target, mapAttribute.Name ?? parameter.Name, type);
			}

			var bindings = parameter.GetAttribute<BindingsAttribute>();
			if (bindings != null)
			{
				return ctx =>
				{
					var binding = bindings.GetBinding(ctx.Request.Method, parameter.Name);
					return MapParameter(ctx, binding.Source, binding.Name, type);
				};
			}

			if (IsPrimitive(type) || type.IsNullable() && IsPrimitive(Nullable.GetUnderlyingType(type)))
			{
				return ctx =>
				{
					var val = FindParameterValue(ctx, parameter.Name);
					// TODO default value attribute
					return val.ToType(type);
				};
			}

			if (type.IsArray && IsPrimitive(type.GetElementType()))
			{
				var itemType = type.GetElementType();
				return ctx =>
				{
					var val = FindParameterValue(ctx, parameter.Name);
					var strVal = val as string;
					if (!string.IsNullOrEmpty(strVal))
					{
						return strVal.Split(',').Select(item => item.ToType(itemType)).ToArrayOfType(itemType);
					}
					var jarray = val as JArray;
					if (null != jarray)
					{
						return jarray.Values<object>().Select(item => item.ToType(itemType)).ToArrayOfType(itemType);
					}
					// TODO default value attribute
					return null;
				};
			}

			return RequestMapper.Build(type);
		}

		private static object MapParameter(IOwinContext ctx, RequestElement source, string parameterName, Type type)
		{
			switch (source)
			{
				case RequestElement.Route:
					return ctx.GetRouteValue(parameterName).UnescapeUriString().ToType(type);
				case RequestElement.Query:
					return ctx.Request.Query.Get(parameterName).UnescapeUriString().ToType(type);
				case RequestElement.Header:
					return ctx.Request.Headers.Get(parameterName).ToType(type);
				case RequestElement.Body:
					return ctx.GetBodyValue(parameterName).ToType(type);
				default:
					throw new NotSupportedException();
			}
		}

		private static object FindParameterValue(IOwinContext ctx, string parameterName)
		{
			var s = ctx.GetRouteValue(parameterName);
			if (!string.IsNullOrEmpty(s))
			{
				return s.UnescapeUriString();
			}

			s = ctx.Request.Query.Get(parameterName);
			if (!string.IsNullOrEmpty(s))
			{
				return s.UnescapeUriString();
			}

			if (!ctx.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
			{
				return ctx.GetBodyValue(parameterName);
			}
			return null;
		}

		private static string UnescapeUriString(this string s)
		{
			return !string.IsNullOrEmpty(s) ? Uri.UnescapeDataString(s) : s;
		}

		private static object GetBodyValue(this IOwinContext ctx, string name)
		{
			var jsonBody = ctx.JsonBody() as JObject;
			if (null != jsonBody)
			{
				var value = jsonBody.GetValue(name, StringComparison.InvariantCultureIgnoreCase);
				if (null != value)
					return value.ToObject<object>();
			}
			return null;
		}

		private static bool IsPrimitive(Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean:
				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
				case TypeCode.DateTime:
				case TypeCode.String:
					return true;
				default:
					if (typeof (Guid) == type)
						return true;
					return false;
			}
		}
	}
}
