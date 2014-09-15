using System;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;

namespace Owin.Routing
{
	// TODO support properties, fields

	public static partial class RoutingApi
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
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				.Where(m => m.GetAttribute<RouteAttribute>() != null)
				.ToList();

			methods.ForEach(method =>
			{
				var invoke = DynamicMethods.CompileMethod(typeof(T), method);
				var resolver = ParameterMapper.Build(method);

				method.GetAttributes<RouteAttribute>().ToList().ForEach(attr =>
				{
					foreach (var verb in attr.Methods)
					{
						app.Route(attr.Url).Register(verb, async ctx =>
						{
							var args = resolver(ctx);
							var instance = method.IsStatic ? (object) null : getInstance(ctx);
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
