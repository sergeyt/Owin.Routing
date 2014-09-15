using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.Owin;

namespace Owin.Routing
{
	using AppFunc = Func<IOwinContext, Func<Task>, Task>;
	using HandlerFunc = Func<IOwinContext, Task>;

	internal static class Keys
	{
		public const string Routes = "owin.routing.routes";
		public const string RouteData = "owin.routing.data";
		public const string HttpContext = "owin.routing.httpctx";
		public const string RequestBytes = "req.bytes";
		public const string JsonBody = "req.json";
	}

	/// <summary>
	/// Provides basic routing API.
	/// </summary>
	public static partial class RoutingApi
	{
		/// <summary>
		/// Injects route to app pipeline.
		/// </summary>
		/// <param name="app">The instance of <see cref="IAppBuilder"/>.</param>
		/// <param name="url">The url template.</param>
		public static RouteBuilder Route(this IAppBuilder app, string url)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");

			var routes = app.Properties.Get<IDictionary<string,Route>>(Keys.Routes);
			if (routes == null)
			{
				routes = new Dictionary<string, Route>(StringComparer.InvariantCultureIgnoreCase);
				app.Properties[Keys.Routes] = routes;
			}

			Route route;
			if (!routes.TryGetValue(url, out route))
			{
				var builder = new RouteBuilder(app);
				route = new Route(url, builder);
				builder.Route = route;
				routes[url] = route;
			}

			return (RouteBuilder) route.RouteHandler;
		}

		#region Shortcuts

		/// <summary>
		/// Registers GET handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Get(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Get(handler);
			return app;
		}

		/// <summary>
		/// Registers GET handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Get(this IAppBuilder app, string url, AppFunc handler)
		{
			app.Route(url).Get(handler);
			return app;
		}

		/// <summary>
		/// Registers POST handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Post(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Post(handler);
			return app;
		}

		/// <summary>
		/// Registers POST handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Post(this IAppBuilder app, string url, AppFunc handler)
		{
			app.Route(url).Post(handler);
			return app;
		}

		/// <summary>
		/// Registers PUT handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Put(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Put(handler);
			return app;
		}

		/// <summary>
		/// Registers PUT handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Put(this IAppBuilder app, string url, AppFunc handler)
		{
			app.Route(url).Put(handler);
			return app;
		}

		/// <summary>
		/// Registers UPDATE handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Update(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Update(handler);
			return app;
		}

		/// <summary>
		/// Registers UPDATE handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Update(this IAppBuilder app, string url, AppFunc handler)
		{
			app.Route(url).Update(handler);
			return app;
		}

		/// <summary>
		/// Registers PATCH handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Patch(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Patch(handler);
			return app;
		}

		/// <summary>
		/// Registers PATCH handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Patch(this IAppBuilder app, string url, AppFunc handler)
		{
			app.Route(url).Patch(handler);
			return app;
		}

		/// <summary>
		/// Registers DELETE handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Delete(this IAppBuilder app, string url, HandlerFunc handler)
		{
			app.Route(url).Delete(handler);
			return app;
		}

		/// <summary>
		/// Registers DELETE handler.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="url"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public static IAppBuilder Delete(this IAppBuilder app, string url, AppFunc handler)
		{
			app.Route(url).Delete(handler);
			return app;
		}

		#endregion
	}
}
