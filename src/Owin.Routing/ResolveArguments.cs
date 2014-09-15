using System;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;

namespace Owin.Routing
{
	/// <summary>
	/// Builds function to resolve method arguments from <see cref="IOwinContext"/>.
	/// </summary>
	internal static class ResolveArguments
	{
		/// <summary>
		/// Builds function to resolve method arguments from <see cref="IOwinContext"/>.
		/// </summary>
		public static Func<IOwinContext, JObject, object[]> Compile(MethodInfo method)
		{
			if (method == null) throw new ArgumentNullException("method");

			var args = (from p in method.GetParameters()
				let meta = p.GetAttribute<ParameterAttribute>()
				select new Func<IOwinContext, JObject, object>((ctx, json) => ResolveArgument(ctx, json, p, meta))
				).ToArray();

			return (ctx, json) => args.Select(f => f(ctx, json)).ToArray();
		}

		private static object ResolveArgument(IOwinContext ctx, JObject json, ParameterInfo parameter, ParameterAttribute bindings)
		{
			if (bindings == null)
			{
				if (string.Equals(ctx.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
				{
					return json.Value<object>(parameter.Name).ToType(parameter.ParameterType);
				}
				return ctx.GetRouteValue(parameter.Name).ToType(parameter.ParameterType);
			}

			var binding = bindings.GetBinding(ctx.Request.Method, parameter.Name);
			switch (binding.Source)
			{
				case RequestElement.Route:
					return ctx.GetRouteValue(binding.Name).ToType(parameter.ParameterType);
				case RequestElement.Query:
					return ctx.Request.Query.Get(binding.Name).ToType(parameter.ParameterType);
				case RequestElement.Header:
					return ctx.Request.Headers.Get(binding.Name).ToType(parameter.ParameterType);
				case RequestElement.Body:
					return json.Value<object>(binding.Name).ToType(parameter.ParameterType);
				default:
					throw new NotSupportedException();
			}
		}
	}
}
