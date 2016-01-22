using System;
using System.Collections.Generic;
using System.Net.Http;
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
					.Get(async ctx =>
					{
						var name = ctx.GetRouteValue("collection");
						await ctx.Response.WriteAsync(name);
					});

				app.Route("docs/{collection}/{id}")
					.Get(async ctx =>
					{
						var col = ctx.GetRouteValue("collection");
						var id = ctx.GetRouteValue("id");
						await ctx.Response.WriteAsync(string.Format("{0}[{1}]", col, id));
					});
			}))
			{
				var response = await server.HttpClient.GetAsync(path);
				return await response.Content.ReadAsStringAsync();
			}
		}

		[TestCase("/docs/reports/1", Result = "reports[1]")]
		public async Task<string> Chain(string path)
		{
			using (var server = TestServer.Create(app =>
			{
				app.Route("docs/{collection}/{id}")
					.Get(async (ctx, next) =>
					{
						var col = ctx.GetRouteValue("collection");
						var id = ctx.GetRouteValue("id");
						ctx.Set("doc-path", string.Format("{0}[{1}]", col, id));
						await next();
					});

				app.Route("docs/{collection}/{id}")
					.Get(async ctx =>
					{
						var s = ctx.Get<string>("doc-path");
						await ctx.Response.WriteAsync(s);
					});
			}))
			{
				var response = await server.HttpClient.GetAsync(path);
				return await response.Content.ReadAsStringAsync();
			}
		}

		[TestCase("GET", "/docs/tags/1", Result = "generic:tags[1]")]
		[TestCase("POST", "/docs/tags/1", Result = "specific:tags[1]")]
		public async Task<string> FromSpecificToGeneral(string method, string path)
		{
			using (var server = TestServer.Create(app =>
			{
				app.Route("docs/tags/{id}")
					.Post(async ctx =>
					{
						var id = ctx.GetRouteValue("id");
						await ctx.Response.WriteAsync(string.Format("specific:tags[{0}]", id));
					});

				app.Route("docs/{collection}/{id}")
					.Get(async ctx =>
					{
						var col = ctx.GetRouteValue("collection");
						var id = ctx.GetRouteValue("id");
						await ctx.Response.WriteAsync(string.Format("generic:{0}[{1}]", col, id));
					});
			}))
			{
				if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
				{
					var response = await server.HttpClient.GetAsync(path);
					return await response.Content.ReadAsStringAsync();
				}
				else
				{
					var response = await server.HttpClient.PostAsync(path, new StringContent("test"));
					return await response.Content.ReadAsStringAsync();
				}
			}
		}

		[Test]
		public void ShouldNotThrowOnRegisteringFewHandlersForSameVerb()
		{
			var app = new Mock<IAppBuilder>();
			app.Setup(x => x.Properties).Returns(new Dictionary<string, object>());

			Assert.DoesNotThrow(() =>
			{
				app.Object.Route("a")
					.Get(async _ => { })
					.Get(async _ => { });
			});
		}

		[TestCase("/docs/reports", Result = "reports")]
		[TestCase("/docs/reports/1", Result = "reports[1]")]
		[TestCase("/api/reports", Result = "reports")]
		[TestCase("/api/reports/1", Result = "reports[1]")]
		public async Task<string> MapShouldWork(string path)
		{
			Action<IAppBuilder> api = app =>
			{
				app.Route("{collection}")
					.Get(async ctx =>
					{
						var name = ctx.GetRouteValue("collection");
						await ctx.Response.WriteAsync(name);
					});

				app.Route("{collection}/{id}")
					.Get(async ctx =>
					{
						var col = ctx.GetRouteValue("collection");
						var id = ctx.GetRouteValue("id");
						await ctx.Response.WriteAsync(string.Format("{0}[{1}]", col, id));
					});
			};

			using (var server = TestServer.Create(app =>
			{
				app.Map("/docs", api);
				app.Map("/api", api);
			}))
			{
				var response = await server.HttpClient.GetAsync(path);
				return await response.Content.ReadAsStringAsync();
			}
		}

		[TestCase("/docs/tags/1", Result = "specific:tags[1]")]
		public async Task<string> FromSpecificToGeneral(string path)
		{
			using (var server = TestServer.Create(app => app.Route("docs/tags/{id}")
				.Put(async ctx =>
				{
					var id = ctx.GetRouteValue("id");
					await ctx.Response.WriteAsync(string.Format("specific:tags[{0}]", id));
				})))
			{
				var response = await server.HttpClient.PutAsync(path, new StringContent("test"));
				return await response.Content.ReadAsStringAsync();
			}
		}
    }
}
