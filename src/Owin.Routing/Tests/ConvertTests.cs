#if NUNIT
using NUnit.Framework;

namespace Owin.Routing.Tests
{
	[TestFixture]
	public class ConvertTests
	{
		[Test]
		public void TestToType()
		{
			// TODO more tests
			// some stupid tests
			Assert.AreEqual("1", "1".ToType<string>());
			Assert.AreEqual(1, "1".ToType<int>());
		}
	}
}
#endif