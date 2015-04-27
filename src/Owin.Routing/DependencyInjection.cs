using System;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;

namespace Owin.Routing
{
	/// <summary>
	/// Small dependency injection implementation.
	/// </summary>
	public static class DependencyInjection
	{
		internal static Func<IOwinContext, T> CompileInitializer<T>()
		{
			var init = CompileInitializer(typeof(T));
			return ctx => (T) init(ctx);
		}

		internal static Func<IOwinContext, object> CompileInitializer(Type type)
		{
			var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
			if (ctors.Length > 1)
			{
				throw new InvalidOperationException(string.Format(
					"Dependency injector cannot create instance for type {0} since it has multiple constructors.",
					type.FullName));
			}

			var ctor = ctors[0];
			var create = DynamicMethods.CompileConstructor(ctor);
			var paramTypes = (from p in ctor.GetParameters() select p.ParameterType).ToArray();

			var props = (from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				where p.GetIndexParameters().Length == 0
				let setter = DynamicMethods.CompileSetter(type, p)
				where setter != null
				select new
				{
					Type = p.PropertyType,
					Setter = setter
				}).ToArray();

			return ctx =>
			{
				var serviceProvider = GetServiceProvider(ctx);
				Func<Type, object> resolveDep = t =>
				{
					if (t == typeof(IServiceProvider)) return serviceProvider;
					if (t == typeof(IOwinContext)) return ctx;
					if (t == typeof(IOwinRequest)) return ctx.Request;
					if (t == typeof(IOwinResponse)) return ctx.Response;
					return serviceProvider.GetService(t);
				};

				var args = (from t in paramTypes select resolveDep(t)).ToArray();
				var instance = create(args);

				foreach (var prop in props)
				{
					var value = resolveDep(prop.Type);
					prop.Setter(instance, value);
				}

				return instance;
			};
		}

		private static string _servicesKey = "app.services";

		/// <summary>
		/// Gets or sets key used to get service provider from <see cref="IOwinContext"/>.
		/// </summary>
		public static string ServicesKey
		{
			get { return _servicesKey; }
			set { _servicesKey = value; }
		}

		private static IServiceProvider GetServiceProvider(IOwinContext context)
		{
			return context.Get<IServiceProvider>(ServicesKey) ?? ServiceProviderStub.Instance;
		}

		private sealed class ServiceProviderStub : IServiceProvider
		{
			public static readonly IServiceProvider Instance = new ServiceProviderStub();
			private ServiceProviderStub(){}
			public object GetService(Type serviceType) { return null; }
		}
	}
}