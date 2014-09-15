using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Owin.Routing
{
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
				.Where(m => m.HasAttribute<RouteAttribute>())
				.ToList();

			var props = typeof(T)
				.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.GetIndexParameters().Length == 0 && m.HasAttribute<RouteAttribute>())
				.ToList();

			var fields = typeof(T)
				.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.HasAttribute<RouteAttribute>())
				.ToList();

			methods.ForEach(method =>
			{
				var invoke = DynamicMethods.CompileMethod(typeof(T), method);
				var mapper = ParameterMapper.Build(method);

				method.GetAttributes<RouteAttribute>().ToList().ForEach(attr =>
				{
					foreach (var verb in attr.Methods)
					{
						app.Route(attr.Url).Register(verb, async ctx =>
						{
							var args = mapper(ctx);
							var instance = method.IsStatic ? (object) null : getInstance(ctx);
							var result = invoke(instance, args);
							await ctx.WriteJson(result);
						});
					}
				});
			});

			props.ForEach(property =>
			{
				var attr = property.GetAttribute<RouteAttribute>();

				var getter = DynamicMethods.CompileGetter(typeof(T), property);
				if (getter != null)
				{
					app.Get(attr.Url, async ctx =>
					{
						var instance = getInstance(ctx);
						var result = getter(instance);
						await ctx.WriteJson(result);
					});
				}

				if (property.CanWrite)
				{
					var setter = DynamicMethods.CompileSetter(typeof(T), property);

					Func<IOwinContext, Task> handler = async ctx =>
					{
						// TODO support different mapping strategies
						var instance = getInstance(ctx);
						var value = ctx.JsonBody().ToObject(property.PropertyType, Json.CreateSerializer());
						setter(instance, value);
						await ctx.WriteJson(new {ok = 1});
					};

					app.Post(attr.Url, handler);
					app.Update(attr.Url, handler);
				}
			});

			fields.ForEach(field =>
			{
				var attr = field.GetAttribute<RouteAttribute>();

				var getter = DynamicMethods.CompileGetter(typeof(T), field);
				if (getter != null)
				{
					app.Get(attr.Url, async ctx =>
					{
						var instance = getInstance(ctx);
						var result = getter(instance);
						await ctx.WriteJson(result);
					});
				}

				if (!field.IsLiteral && !field.IsInitOnly)
				{
					var setter = DynamicMethods.CompileSetter(typeof(T), field);

					Func<IOwinContext, Task> handler = async ctx =>
					{
						// TODO support different mapping strategies
						var instance = getInstance(ctx);
						var value = ctx.JsonBody().ToObject(field.FieldType, Json.CreateSerializer());
						setter(instance, value);
						await ctx.WriteJson(new { ok = 1 });
					};

					app.Post(attr.Url, handler);
					app.Update(attr.Url, handler);
				}
			});

			return app;
		}
	}
}
