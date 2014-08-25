using System;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;

namespace Owin.Routing
{
	public static class ReflectionRoutingApi
	{
		/// <summary>
		/// Finds non-static methods marked with <see cref="RouteAttribute"/> and registers routes on reflected methods.
		/// </summary>
		/// <typeparam name="T">Type to reflect.</typeparam>
		/// <param name="app">The OWIN pipeline builder.</param>
		/// <param name="getInstance">Functor to get instance of T.</param>
		public static IAppBuilder RegisterRoutes<T>(this IAppBuilder app, Func<IOwinContext, T> getInstance)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (getInstance == null) throw new ArgumentNullException("getInstance");

			var methods = typeof(T)
				.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(m => m.GetAttribute<RouteAttribute>() != null)
				.ToList();

			methods.ForEach(method =>
			{
				var invoke = DynamicMethods.CompileMethod(typeof(T), method);
				var sig = method.GetParameters();

				method.GetAttributes<RouteAttribute>().ToList().ForEach(attr =>
				{
					foreach (var verb in attr.Methods)
					{
						if (string.Equals(verb, "POST", StringComparison.OrdinalIgnoreCase))
						{
							app.Route(attr.Url).Register(verb, async ctx =>
							{
								var json = await ctx.ReadJObject();
								var args = sig.Select(p => json.Value<object>(p.Name)).ToArray();
								var instance = getInstance(ctx);
								var result = invoke(instance, args);
								await ctx.WriteJson(result);
							});
						}
						else
						{
							app.Route(attr.Url).Register(verb, async ctx =>
							{
								var args = sig.Select(p => (object) ctx.GetRouteValue(p.Name)).ToArray();
								var instance = getInstance(ctx);
								var result = invoke(instance, args);
								await ctx.WriteJson(result);
							});
						}
					}
				});
			});

			return app;
		}
	}
}
