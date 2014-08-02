using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Owin;
using Owin.Routing;

namespace Tests
{
	[TestFixture]
    public class RoutingTests
    {
		[TestCase("/docs/reports", Result = "reports")]
		[TestCase("/docs/reports/1", Result = "reports[1]")]
		public async Task<string> Simple(string path)
		{
			using (var server = TestServer.Create(app =>
			{
				app.Route("docs/{collection}")
					.Get(async (ctx, data) =>
					{
						var name = Convert.ToString(data.Values["collection"]);
						await ctx.Response.WriteAsync(name);
					});

				app.Route("docs/{collection}/{id}")
					.Get(async (ctx, data) =>
					{
						var col = Convert.ToString(data.Values["collection"]);
						var id = Convert.ToString(data.Values["id"]);
						await ctx.Response.WriteAsync(string.Format("{0}[{1}]", col, id));
					});
			}))
			{
				var response = await server.HttpClient.GetAsync(path);
				var s = await response.Content.ReadAsStringAsync();
				return s;
			}
		}

		[Test]
		public void ShouldReturnSameRouteBuilder()
		{
			var app = new Mock<IAppBuilder>();
			app.Setup(x => x.Properties).Returns(new Dictionary<string, object>());
			Assert.AreSame(app.Object.Route("a"), app.Object.Route("a"));
		}

		[Test]
		public void ShouldThrowOnRegisteringFewHandlersForSameVerb()
		{
			var app = new Mock<IAppBuilder>();
			app.Setup(x => x.Properties).Returns(new Dictionary<string, object>());

			Assert.Throws<ArgumentException>(() =>
			{
				app.Object.Route("a")
					.Get(async (context, data) => { })
					.Get(async (context, data) => { });
			});
		}
    }
}
