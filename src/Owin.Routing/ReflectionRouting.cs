using System;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;

namespace Owin.Routing
{
	public static partial class RoutingApi
	{
		public static IAppBuilder UseApi<T>(this IAppBuilder app)
		{
			return app.UseApi(ctx =>
			{
				// TODO dependency injection (pass services to ctor, set properties)
				var instance = Activator.CreateInstance<T>();
				return instance;
			});
		}

		/// <summary>
		/// Finds methods marked with <see cref="RouteAttribute"/> and registers routes on reflected methods.
		/// </summary>
		/// <typeparam name="T">Type to reflect.</typeparam>
		/// <param name="app">The OWIN pipeline builder.</param>
		/// <param name="getInstance">Functor to get instance of T.</param>
		public static IAppBuilder UseApi<T>(this IAppBuilder app, Func<IOwinContext, T> getInstance)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (getInstance == null) throw new ArgumentNullException("getInstance");

			var methods = typeof(T)
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				.Where(m => m.HasAttribute<RouteAttribute>())
				.ToList();

			methods.ForEach(method =>
			{
				var invoke = DynamicMethods.CompileMethod(typeof(T), method);
				var mapper = ParameterMapper.Build(method);

				method.GetAttributes<RouteAttribute>()
					.ToList()
					.ForEach(attr =>
					{
						var verb = GetHttpMethod(method);
						app.Route(attr.Template).Register(verb, async ctx =>
						{
							var args = mapper(ctx);
							var instance = method.IsStatic ? (object) null : getInstance(ctx);
							var result = invoke(instance, args);
							await ctx.WriteJson(result);
						});
					});
			});

			return app;
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
