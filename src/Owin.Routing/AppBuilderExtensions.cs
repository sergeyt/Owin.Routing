using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.Owin;

namespace Owin.Routing
{
	using HandlerFunc = Func<IOwinContext, RouteData, Task>;

	/// <summary>
	/// <see cref="IAppBuilder"/> extensions.
	/// </summary>
	public static class AppBuilderExtensions
	{
		/// <summary>
		/// Injects route to app pipeline.
		/// </summary>
		/// <param name="app">The instance of <see cref="IAppBuilder"/>.</param>
		/// <param name="route">The url pattern.</param>
		public static RouteBuilder Route(this IAppBuilder app, string route)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (string.IsNullOrEmpty(route)) throw new ArgumentNullException("route");

			const string keyRoutes = "app.routes";
			var routes = app.Properties.Get<RouteCollection>(keyRoutes);

			if (routes == null)
			{
				routes = new RouteCollection();
				app.Properties[keyRoutes] = routes;

				app.Use(async (ctx, next) =>
				{
					var data = routes.GetRouteData(new HttpContextImpl(ctx));
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

		public static IAppBuilder Get(this IAppBuilder app, string route, HandlerFunc handler)
		{
			app.Route(route).Get(handler);
			return app;
		}

		public static IAppBuilder Post(this IAppBuilder app, string route, HandlerFunc handler)
		{
			app.Route(route).Post(handler);
			return app;
		}

		public static IAppBuilder Put(this IAppBuilder app, string route, HandlerFunc handler)
		{
			app.Route(route).Put(handler);
			return app;
		}

		public static IAppBuilder Update(this IAppBuilder app, string route, HandlerFunc handler)
		{
			app.Route(route).Update(handler);
			return app;
		}

		public static IAppBuilder Patch(this IAppBuilder app, string route, HandlerFunc handler)
		{
			app.Route(route).Patch(handler);
			return app;
		}

		public static IAppBuilder Delete(this IAppBuilder app, string route, HandlerFunc handler)
		{
			app.Route(route).Delete(handler);
			return app;
		}

		private static T Get<T>(this IDictionary<string, object> dictionary, string key) where T : class
		{
			object value;
			return dictionary.TryGetValue(key, out value) ? value as T : null;
		}
	}	
}
