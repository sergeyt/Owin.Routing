using System;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using NUnit.Framework;
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
    }
}
