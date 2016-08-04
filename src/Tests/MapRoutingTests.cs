using System;
using System.Net.Http;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Owin.Routing;

namespace Tests
{
	[TestFixture]
	public class MapRoutingTests
	{
		[Test]
		public async void CheckAsyncMethod()
		{
			using (var server = TestServer.Create(app => app.Route(r => r.UseApi<ReflectionRoutingTests.AsyncApi>())))
			{
				var s = await server.HttpClient.GetStringAsync("items/123");
				Assert.AreEqual("\"123\"", s);
			}
		}

		[TestCase("item", @"{""Key"":""123""}")]
		[TestCase("array", @"[{""Key"":""123""}]")]
		public async void CheckAsyncMethodInternalApi(string path, string expected)
		{
			using (var server = TestServer.Create(app => app.Route(r => r.UseApi<ReflectionRoutingTests.InternalAsyncApi>())))
			{
				var s = await server.HttpClient.GetStringAsync(path);
				Assert.AreEqual(expected, s);
			}
		}

		[TestCase("GET", "item/abc")]
		[TestCase("PUT", "item/123")]
		public async void CheckParameterMappingErrorHandling(string method, string path)
		{
			using (var server = TestServer.Create(app => app.Route(r => r.UseApi<ReflectionRoutingTests.ApiWithErrorHandler>())))
			{
				string strResponse;
				if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
				{
					strResponse = await server.HttpClient.GetStringAsync("item/abc");
				}
				else
				{
					var content = new StringContent(JObject.FromObject(new { name = "TestItem", value = "3.14" }).ToString());
					var response = await server.HttpClient.PutAsync("item/123", content);
					strResponse = await response.Content.ReadAsStringAsync();
				}
				var result = JObject.Parse(strResponse);
				Assert.That(result.Value<string>("customError"), Is.EqualTo("FormatException"));
			}
		}
	}
}
