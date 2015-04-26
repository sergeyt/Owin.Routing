#if NUNIT
using System;
using Microsoft.Owin;
using Moq;
using NUnit.Framework;

namespace Owin.Routing.Tests
{
	[TestFixture]
	public class DependencyInjectionTests
	{
		[TestCase(typeof(Module1))]
		[TestCase(typeof(Module2))]
		public void TestInjection(Type type)
		{
			var init = DependencyInjection.CompileInitializer(type);

			var logger = Mock.Of<ILogger>();
			var store = Mock.Of<IStore>(s => s.GetItems() == new[] {"a", "b", "c"});

			var sp = Mock.Of<IServiceProvider>(p =>
				p.GetService(typeof(ILogger)) == logger
				&& p.GetService(typeof(IStore)) == store
				);

			var ctx = Mock.Of<IOwinContext>(x => x.Get<IServiceProvider>("app.services") == sp);
			var obj = (IModule) init(ctx);

			Assert.IsNotNull(obj);
			Assert.AreSame(ctx, obj.Context);
			Assert.AreSame(logger, obj.Logger);
			Assert.AreSame(store, obj.Store);
		}

		public interface IModule
		{
			IOwinContext Context { get; }
			ILogger Logger { get; }
			IStore Store { get; }
		}

		public class Module1 : IModule
		{
			public IOwinContext Context { get; private set; }
			public ILogger Logger { get; private set; }
			public IStore Store { get; private set; }

			public Module1(IOwinContext context, ILogger logger, IStore store)
			{
				Context = context;
				Logger = logger;
				Store = store;
			}
		}

		public class Module2 : IModule
		{
			public IOwinContext Context { get; set; }
			public ILogger Logger { get; set; }
			public IStore Store { get; set; }
		}

		public interface ILogger
		{
			void Info(string msg);
		}

		public interface IStore
		{
			string[] GetItems();
		}
	}
}
#endif