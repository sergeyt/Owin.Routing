using System.Collections.Generic;
using System.Web.Routing;
using Microsoft.Owin;

namespace Owin.Routing
{
	/// <summary>
	/// <see cref="IAppBuilder"/> extensions.
	/// </summary>
	internal static class AppBuilderExtensions
	{
		private const string KeyRoutes = "app.routes";

		public static RouteBuilder Route(this IAppBuilder app, string route)
		{
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
						await ((RouteBuilder) data.RouteHandler).Process(ctx, data, next);
					}
					else
					{
						await next();
					}
				});
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
