﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Owin.Routing;

namespace Tests
{
	[TestFixture]
	public class ReflectionRoutingTests
	{
		[Test]
		public async void CheckSimpleApi()
		{
			var licenses = new[] { new LicenseInfo { SerialKey = "serialKey", Package = "package", Status = "activated", DaysLeft = 12 } };

			var lm = Mock.Of<ILicenseManager>(x =>
				x.GetLicenses() == licenses
				&& x.GetActivationKey(It.IsAny<string>()) == "123"
				);

			Mock.Get(lm).Setup(x => x.AddLicense(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string, string>((a, b, c) => a + b + c);

			using (var server = TestServer.Create(app => app.UseApi(_ => lm)))
			{
				var s = await server.HttpClient.GetStringAsync("licenses");
				Assert.AreEqual("[{\"SerialKey\":\"serialKey\",\"Package\":\"package\",\"Status\":\"activated\",\"DaysLeft\":12}]", s);

				s = await server.HttpClient.GetStringAsync("licenses/abc/activationKey");
				Assert.AreEqual("\"123\"", s);

				var res =
					await server.HttpClient.PostAsync("licenses",
						new StringContent(JObject.FromObject(
							new
							{
								serialKey = "1",
								activationKey = "2",
								licenseKey = "3"
							}).ToString())
						);

				s = await res.Content.ReadAsStringAsync();
				Assert.AreEqual("\"123\"", s);
			}
		}

		[Test]
		public async void CheckAsyncMethod()
		{
			using (var server = TestServer.Create(app => app.UseApi<AsyncApi>()))
			{
				var s = await server.HttpClient.GetStringAsync("items/123");
				Assert.AreEqual("\"123\"", s);
			}
		}

		[TestCase("item", @"{""Key"":""123""}")]
		[TestCase("array", @"[{""Key"":""123""}]")]
		public async void CheckAsyncMethodInternalApi(string path, string expected)
		{
			using (var server = TestServer.Create(app => app.UseApi<InternalAsyncApi>()))
			{
				var s = await server.HttpClient.GetStringAsync(path);
				Assert.AreEqual(expected, s);
			}
		}

		[Test]
		public async void CheckParameterMappingErrorHandling()
		{
			using (var server = TestServer.Create(app => app.UseApi<ApiWithErrorHandler>()))
			{
				var s = await server.HttpClient.GetStringAsync("item/abc");
				var result = JObject.Parse(s);
				Assert.That(result.Value<string>("customError"), Is.EqualTo("FormatException"));
			}
		}

		public class AsyncApi
		{
			[Route("items/{key}")]
			public async Task<string> GetItem(string key)
			{
				await Task.Delay(1);
				return key;
			}
		}

		internal class InternalAsyncApi
		{
			[Route("item")]
			public async Task<Internal> GetResult()
			{
				return await Task.FromResult(new Internal());
			}

			[Route("array")]
			public async Task<Internal[]> GetArray()
			{
				return await Task.FromResult(new []{ new Internal() });
			}
		}

		internal class Internal
		{
			public string Key { get { return "123"; } }
		}

		public class ApiWithErrorHandler
		{
			[Route("item/{number}")]
			public void GetItem(int number)
			{
			}

			[ErrorHandler]
			public static object OnError(IOwinContext ctx, Exception error)
			{
				return new {customError = error.GetType().Name};
			}
		}

		public interface ILicenseManager
		{
			[Route("licenses")]
			IEnumerable<LicenseInfo> GetLicenses();

			[Route("licenses/{serialKey}/activationKey")]
			string GetActivationKey(string serialKey);

			[Route("licenses")]
			string AddLicense(string serialKey, string activationKey, string licenseKey);

			[Route("licenses/{serialKey}")]
			void RemoveLicense(string serialKey);

			[HttpGet]
			[Route("licenses/{subjectId}/activate")]
			void Activate(Guid subjectId);

			[HttpGet]
			[Route("licenses/{subjectId}/deactivate")]
			void Deactivate(Guid subjectId);

			[Route("licenses/{subjectId}/status")]
			string GetLicenseStatus(Guid subjectId);

			[Route("licenses/{subjectId}")]
			LicenseInfo GetLicenseInfo(Guid subjectId);
		}

		public sealed class LicenseInfo
		{
			public string SerialKey { get; set; }
			public string Package { get; set; }
			public string Status { get; set; }
			public int DaysLeft { get; set; }
		}
	}
}
