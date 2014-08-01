using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Microsoft.Owin;

namespace Owin.Routing
{
	/// <summary>
	/// <see cref="IAppBuilder"/> extensions.
	/// </summary>
	public static class AppBuilderExtensions
	{
		private const string KeyRoutes = "app.routes";

		/// <summary>
		/// Gets <see cref="RouteBuilder"/> for given route.
		/// </summary>
		/// <param name="app">The instance of <see cref="IAppBuilder"/>.</param>
		/// <param name="route">The route url.</param>
		public static RouteBuilder Route(this IAppBuilder app, string route)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (string.IsNullOrEmpty(route)) throw new ArgumentNullException("route");

			var routes = app.Properties.Get<RouteCollection>(KeyRoutes);

			if (routes == null)
			{
				routes = new RouteCollection();
				app.Properties[KeyRoutes] = routes;

				app.Use(async (ctx, next) =>
				{
					var data = app.ResolveRoute(ctx);
					if (data != null && data.RouteHandler is RouteBuilder)
					{
						await ((RouteBuilder) data.RouteHandler).Invoke(ctx, data, next);
					}
					else
					{
						await next();
					}
				});
			}

			var existing = routes.OfType<Route>().FirstOrDefault(r => string.Equals(r.Url, route, StringComparison.InvariantCultureIgnoreCase));
			if (existing != null)
			{
				return (RouteBuilder) existing.RouteHandler;
			}

			var builder = new RouteBuilder();
			routes.Add(new Route(route, builder));
			return builder;
		}

		private static T Get<T>(this IDictionary<string, object> dictionary, string key) where T : class
		{
			object value;
			return dictionary.TryGetValue(key, out value) ? value as T : null;
		}

		private static RouteData ResolveRoute(this IAppBuilder app, IOwinContext context)
		{
			var routes = app.Properties.Get<RouteCollection>(KeyRoutes);
			if (routes == null) return null;
			return routes.GetRouteData(new HttpContextImpl(context));
		}
	}	
}
