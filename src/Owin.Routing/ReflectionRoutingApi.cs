using System;
using System.Linq;
using System.Reflection;

namespace Owin.Routing
{
	public static class ReflectionRoutingApi
	{
		public static IAppBuilder RegisterRoutes<T>(this IAppBuilder app, Func<T> getInstance)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (getInstance == null) throw new ArgumentNullException("getInstance");

			var methods = typeof(T)
				.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(m => m.GetAttribute<RouteAttribute>(true) != null)
				.ToList();

			methods.ForEach(method =>
			{
				method.GetAttributes<RouteAttribute>().ToList().ForEach(attr =>
				{
					foreach (var verb in attr.Methods)
					{
						app.Route(attr.Url).Register(verb, async (context, data) =>
						{
							// TODO parse args from url template
							// TODO for POST parse JSON
							throw new NotImplementedException();
						});
					}
				});
			});

			return app;
		}
	}
}
