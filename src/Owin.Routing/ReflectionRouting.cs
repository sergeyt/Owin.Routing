using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Owin.Routing
{
	public static partial class RoutingApi
	{
		/// <summary>
		/// Registers methods annotated with <see cref="RouteAttribute"/> into routing pipeline.
		/// </summary>
		/// <typeparam name="T">Type to reflect.</typeparam>
		/// <param name="app">The OWIN pipeline builder.</param>
		public static IAppBuilder UseApi<T>(this IAppBuilder app)
		{
			var init = DependencyInjection.CompileInitializer<T>();
			return app.UseApi(init);
		}

		/// <summary>
		/// Registers methods annotated with <see cref="RouteAttribute"/> into routing pipeline.
		/// </summary>
		/// <typeparam name="T">Type to reflect.</typeparam>
		/// <param name="app">The OWIN pipeline builder.</param>
		/// <param name="getInstance">Function to get instance of T.</param>
		public static IAppBuilder UseApi<T>(this IAppBuilder app, Func<IOwinContext, T> getInstance)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (getInstance == null) throw new ArgumentNullException("getInstance");

			var type = typeof(T);
			var prefixAttr = type.GetAttribute<RoutePrefixAttribute>();
			var prefix = prefixAttr != null ? prefixAttr.Prefix : string.Empty;

			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

			var actions = (from m in type.GetMethods(bindingFlags)
				let route = m.GetAttribute<RouteAttribute>()
				where route != null
				select new {Method = m, Route = route}).ToList();

			actions.ForEach(a =>
			{
				var invoke = DynamicMethods.CompileMethod(type, a.Method);
				var mapper = ParameterMapper.Build(a.Method);
				var returnType = a.Method.ReturnType;
				var isAsync = returnType == typeof(Task) ||
				              (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>));
				var hasResult = !(returnType == typeof(void) || returnType == typeof(Task));

				var verb = GetHttpMethod(a.Method);
				var pattern = AddPrefix(prefix, a.Route.Template);

				app.Route(pattern).Register(verb, async ctx =>
				{
					string error;
					var args = MapParameters(ctx, mapper, out error);
					if (error != null)
					{
						ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
						await ctx.WriteJson(new { error });
						return;
					}

					var instance = a.Method.IsStatic ? (object) null : getInstance(ctx);
					var result = invoke(instance, args);
					if (isAsync)
					{
						if (hasResult)
						{
							result = await (dynamic) result;
						}
						else
						{
							await (dynamic) result;
						}
					}
					if (hasResult)
					{
						await ctx.WriteJson(result);
					}
				});
			});

			return app;
		}

		private static object[] MapParameters(IOwinContext ctx, Func<IOwinContext, object[]> mapper, out string error)
		{
			error = null;
			try
			{
				return mapper(ctx);
			}
			catch (FormatException e)
			{
				error = e.Message;
				return null;
			}
		}

		private static string AddPrefix(string prefix, string pattern)
		{
			if (string.IsNullOrEmpty(prefix)) return pattern;
			return prefix.TrimEnd('/') + '/' + pattern.TrimStart('/');
		}

		private static string GetHttpMethod(MethodInfo method)
		{
			var attr = method.GetAttribute<HttpMethodAttribute>();
			if (attr != null)
			{
				return attr.Method;
			}

			var name = method.Name;
			if (name.StartsWith("Create", StringComparison.OrdinalIgnoreCase)
				|| name.StartsWith("Add", StringComparison.OrdinalIgnoreCase)
				|| name.StartsWith("Insert", StringComparison.OrdinalIgnoreCase))
			{
				return HttpMethod.Post;
			}

			if (name.StartsWith("Update", StringComparison.OrdinalIgnoreCase))
			{
				return HttpMethod.Put;
			}

			if (name.StartsWith("Patch", StringComparison.OrdinalIgnoreCase))
			{
				return HttpMethod.Patch;
			}

			if (name.StartsWith("Remove", StringComparison.OrdinalIgnoreCase)
			    || name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase))
			{
				return HttpMethod.Delete;
			}

			return HttpMethod.Get;
		}
	}
}
