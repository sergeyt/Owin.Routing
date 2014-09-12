using System;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin;

namespace Owin.Routing
{
	/// <summary>
	/// Injects <see cref="IOwinContext"/> extensions like GetRouteValue.
	/// </summary>
	public static class OwinContextRoutingExtensions
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

		internal static HttpContextBase HttpContext(this IOwinContext ctx)
		{
			var httpContext = ctx.Get<HttpContextBase>(Keys.HttpContext);
			if (httpContext == null)
			{
				httpContext = new HttpContextImpl(ctx);
				ctx.Set(Keys.HttpContext, httpContext);
			}
			return httpContext;
		}
	}
}
