using System;
using System.Collections.Generic;
using System.Net.Http;
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
		public async void Test()
		{
			var lm = new Mock<ILicenseManager>();
			lm.Setup(x => x.GetLicenses())
				.Returns(new[] {new LicenseInfo {SerialKey = "serialKey", Package = "package", Status = "activated", DaysLeft = 12}});
			lm.Setup(x => x.GetActivationKey(It.IsAny<string>()))
				.Returns<string>(s => "123");
			lm.Setup(x => x.AddLicense(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string, string>((a, b, c) => a + b + c);

			using (var server = TestServer.Create(app => app.UseApi(_ => lm.Object)))
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
