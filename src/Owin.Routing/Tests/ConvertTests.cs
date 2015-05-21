#if NUNIT
using System;
using NUnit.Framework;

namespace Owin.Routing.Tests
{
	[TestFixture]
	public class ConvertTests
	{
		[Test]
		public void TestToType()
		{
			object nil = null;

			Assert.AreEqual("1", "1".ToType<string>());
			Assert.AreEqual(1, "1".ToType<int>());

			var guid = Guid.NewGuid();
			Assert.AreEqual(guid, guid.ToString().ToType<Guid>());

			Assert.AreEqual(ItemKind.Simple, "Simple".ToType<ItemKind>());
			Assert.AreEqual(ItemKind.Simple, 0.ToType<ItemKind>());
			Assert.AreEqual(ItemKind.Simple, "0".ToType<ItemKind>());
			Assert.AreEqual(ItemKind.Simple, nil.ToType<ItemKind>());

			var nullable = "123".ToType(typeof (int?));
			Assert.AreEqual(nullable, 123);
		}

		private enum ItemKind { Simple, Complex }
	}
}
#endif