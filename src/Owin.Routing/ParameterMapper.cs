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

			var bindings = parameter.GetAttribute<BindingsAttribute>();
			if (bindings != null)
			{
				return ctx =>
				{
					var binding = bindings.GetBinding(ctx.Request.Method, parameter.Name);
					switch (binding.Source)
					{
						case RequestElement.Route:
							return ctx.GetRouteValue(binding.Name).ToType(type);
						case RequestElement.Query:
							return ctx.Request.Query.Get(binding.Name).ToType(type);
						case RequestElement.Header:
							return ctx.Request.Headers.Get(binding.Name).ToType(type);
						case RequestElement.Body:
							return ctx.JsonBody().Value<object>(binding.Name).ToType(type);
						default:
							throw new NotSupportedException();
					}
				};
			}

			if (IsPrimitive(type))
			{
				return ctx =>
				{
					var s = ctx.GetRouteValue(parameter.Name);
					if (!string.IsNullOrEmpty(s))
					{
						return s.ToType(type);
					}

					s = ctx.Request.Query.Get(parameter.Name);
					if (!string.IsNullOrEmpty(s))
					{
						return s.ToType(type);
					}

					if (!ctx.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
					{
						return ctx.JsonBody().Value<object>(parameter.Name).ToType(type);
					}

					// TODO default value attribute
					return s.ToType(type);
				};
			}

			return RequestMapper.Build(type);
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
					return false;
			}
		}
	}
}
