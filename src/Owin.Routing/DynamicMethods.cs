using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Owin.Routing
{
	/// <summary>
	/// API to compile dynamic calls of properties, methods.
	/// </summary>
	internal static class DynamicMethods
	{
		public static Func<object, object> CompileGetter(Type type, PropertyInfo property)
		{
			var target = Expression.Parameter(typeof(object), "target");
			var instance = Expression.Convert(target, type);

			var value = Expression.Convert(Expression.Property(instance, property), typeof(object));
			var lambda = Expression.Lambda<Func<object, object>>(value, target);

			return lambda.Compile();
		}

		public static Action<object, object> CompileSetter(Type type, PropertyInfo property)
		{
			var method = property.GetSetMethod();
			if (method == null) return null;

			var target = Expression.Parameter(typeof(object), "target");
			var value = Expression.Parameter(typeof(object), "value");

			var instance = Expression.Convert(target, type);
			var val = Expression.Convert(value, property.PropertyType);

			var call = Expression.Call(instance, method, val);
			var lambda = Expression.Lambda<Action<object, object>>(call, target, value);

			return lambda.Compile();
		}

		public static Func<object, int, object> CompileIndexer(Type type)
		{
			var property = type
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(pi =>
				{
					var p = pi.GetIndexParameters();
					return p.Length == 1 && p[0].ParameterType == typeof(int);
				});

			if (property == null) return null;

			var method = property.GetGetMethod();
			if (method == null) return null;

			return CompileMethodWithIndex(type, method);
		}

		public static Func<object, int, object> CompileRemoveAt(Type type)
		{
			var method = type.GetMethod("RemoveAt", new[] {typeof(int)});
			return method == null ? null : CompileMethodWithIndex(type, method);
		}

		private static Func<object, int, object> CompileMethodWithIndex(Type type, MethodInfo method)
		{
			var target = Expression.Parameter(typeof(object), "target");
			var index = Expression.Parameter(typeof(int), "index");

			var instance = Expression.Convert(target, type);

			var call = Expression.Call(instance, method, index);
			if (method.ReturnType == typeof(void))
			{
				var lambda = Expression.Lambda<Action<object, int>>(call, target, index);
				var action = lambda.Compile();
				return (o, i) =>
				{
					action(o, i);
					return null;
				};
			}
			else
			{
				var lambda = Expression.Lambda<Func<object, int, object>>(call, target, index);
				return lambda.Compile();
			}
		}

		public static Func<object, object[], object> CompileAdd(Type type)
		{
			var method = type.GetMethod("Add");
			if (method == null) return null;

			return CompileMethod(type, method);
		}

		public static Func<object, object[], object> CompileMethod(Type type, MethodInfo method)
		{
			var target = Expression.Parameter(typeof(object), "target");
			var args = Expression.Parameter(typeof(object[]), "args");

			var instance = Expression.Convert(target, type);
			var parameters = method.GetParameters();
			var callArgs = parameters.Select((p, i) =>
			{
				var item = Expression.ArrayIndex(args, Expression.Constant(i));
				var val = Convert(item, p.ParameterType);
				return (Expression) Expression.Convert(val, p.ParameterType);
			});

			if (method.ReturnType == typeof(void))
			{
				var call = Expression.Call(instance, method, callArgs);
				var lambda = Expression.Lambda<Action<object, object[]>>(call, target, args);
				var action = lambda.Compile();
				return (o, a) =>
				{
					action(o, a);
					return null;
				};
			}
			else
			{
				var call = Expression.Call(instance, method, callArgs);
				var lambda = Expression.Lambda<Func<object, object[], object>>(call, target, args);
				return lambda.Compile();
			}
		}

		private static MethodInfo _toTypeMethod;

		private static Expression Convert(Expression value, Type type)
		{
			var toType = _toTypeMethod ??
			             (_toTypeMethod = typeof(ConvertExtensions).GetMethod("ToType", new[] {typeof(object), typeof(Type)}));
			return Expression.Call(toType, value, Expression.Constant(type));
		}
	}
}
