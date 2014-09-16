using System;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;

namespace Owin.Routing
{
	/// <summary>
	/// Implements mapping from <see cref="IOwinContext"/> to instance of .NET type.
	/// </summary>
	internal static class RequestMapper
	{
		/// <summary>
		/// Builds object mapper from OWIN environment for given .NET type.
		/// </summary>
		/// <param name="type">The type of object to map.</param>
		public static Func<IOwinContext, object> Build(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
			var ctor = ctors.FirstOrDefault(c => c.GetParameters().Any(p => p.HasAttribute<MapAttribute>()));
			if (ctor != null)
			{
				var args = ParameterMapper.Build(ctor);
				// TODO build expression tree to create instance
				return ctx => ctor.Invoke(null, args(ctx));
			}

			var props = type
				.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.Where(p => p.CanWrite && p.GetIndexParameters().Length == 0 && p.HasAttribute<MapAttribute>())
				.ToArray();

			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.Where(f => !f.IsLiteral && !f.IsInitOnly && f.HasAttribute<MapAttribute>())
				.ToArray();

			var setters = props
				.Select(p => Setter(p))
				.Concat(fields.Select(f => Setter(f)))
				.ToArray();

			return ctx =>
			{
				var instance = Activator.CreateInstance(type);

				foreach (var setter in setters)
				{
					setter(ctx, instance);
				}

				return instance;
			};
		}

		private static Action<IOwinContext, object> Setter(MemberInfo member)
		{
			var attr = member.GetAttribute<MapAttribute>();
			var name = string.IsNullOrEmpty(attr.Name) ? member.Name : attr.Name;
			var type = member is PropertyInfo ? ((PropertyInfo) member).PropertyType : ((FieldInfo) member).FieldType;

			var getter = Getter(attr.Target, name, type);
			var setter = DynamicMethods.CompileSetter(member.DeclaringType, member);

			return (ctx, instance) =>
			{
				var value = getter(ctx);
				if (value == null) return;
				setter(instance, value);
			};
		}

		private static Func<IOwinContext, object> Getter(RequestElement element, string name, Type type)
		{
			switch (element)
			{
				case RequestElement.Route:
					return ctx => ctx.GetRouteValue(name).ToType(type);
				case RequestElement.Query:
					return ctx => ctx.Request.Query.Get(name).ToType(type);
				case RequestElement.Header:
					return ctx => ctx.Request.Headers.Get(name).ToType(type);
				case RequestElement.Body:
					return ctx => ctx.JsonBody().Value<object>(name).ToType(type);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
