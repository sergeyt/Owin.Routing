using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin;

namespace Owin.Routing
{
	using HandlerFunc = Func<IOwinContext, Task>;

	internal static class Keys
	{
		public const string Routes = "app.routes";
		public const string RouteData = "app.route.data";
	}

	/// <summary>
	/// Provides basic routing API.
	/// </summary>
	public static class RoutingApi
	{
		/// <summary>
		/// Injects route to app pipeline.
		/// </summary>
		/// <param name="app">The instance of <see cref="IAppBuilder"/>.</param>
		/// <param name="url">The url template.</param>
		public static RouteBuilder Route(this IAppBuilder app, string url)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");

			var routes = app.Properties.Get<RouteCollection>(Keys.Routes);

			if (routes == null)
			{
				routes = new RouteCollection();
				app.Properties[Keys.Routes] = routes;

				app.Use(async (ctx, next) =>
				{
					RouteData data;
					using (routes.GetReadLock())
					{
						var httpContext = new HttpContextImpl(ctx);
						data = routes.Select(r => GetRouteData(r, httpContext)).FirstOrDefault(d => d != null);
					}

					if (data != null)
					{
						ctx.Set(Keys.RouteData, data);
						await ((RouteBuilder) data.RouteHandler).Invoke(ctx);
					}
					else
					{
						await next();
					}
				});
			}

			var existing = routes.OfType<Route>().FirstOrDefault(r => string.Equals(r.Url, url, StringComparison.InvariantCultureIgnoreCase));
			if (existing != null)
			{
				return (RouteBuilder) existing.RouteHandler;
			}

			var builder = new RouteBuilder();
			routes.Add(new Route(url, builder));
			return builder;
		}

		private static RouteData GetRouteData(RouteBase route, HttpContextBase httpContext)
		{
			var data = route.GetRouteData(httpContext);
			if (data == null) return null;
			var builder = data.RouteHandler as RouteBuilder;
			if (builder == null) return null;
			return builder.HasHandler(httpContext.Request.HttpMethod) ? data : null;
		}

		public static IAppBuilder Get(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Get(handler);
			return app;
		}

		public static IAppBuilder Post(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Post(handler);
			return app;
		}

		public static IAppBuilder Put(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Put(handler);
			return app;
		}

		public static IAppBuilder Update(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Update(handler);
			return app;
		}

		public static IAppBuilder Patch(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Patch(handler);
			return app;
		}

		public static IAppBuilder Delete(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Delete(handler);
			return app;
		}
	}

	public static class OwinContextExtensions
	{
		public static T GetRouteValue<T>(this IOwinContext context, string name)
		{
			var data = context.Get<RouteData>(Keys.RouteData);
			if (data == null) throw new InvalidOperationException();
			var val = data.Values[name];
			return val.ToType<T>();
		}

		public static string GetRouteValue(this IOwinContext context, string name)
		{
			return context.GetRouteValue<string>(name);
		}
	}
}
