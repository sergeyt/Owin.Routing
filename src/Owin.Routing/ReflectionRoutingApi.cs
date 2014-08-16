using System;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;

namespace Owin.Routing
{
	public static class ReflectionRoutingApi
	{
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
						app.Route(attr.Url).Register(verb, async ctx =>
						{
							// TODO for POST requests parse args from JSON input
							var args = sig.Select(p => (object) ctx.GetRouteValue(p.Name)).ToArray();
							var instance = getInstance(ctx);
							var result = invoke(instance, args);
							await ctx.WriteJson(result);
						});
					}
				});
			});

			return app;
		}
	}
}
