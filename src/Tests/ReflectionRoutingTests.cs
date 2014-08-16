using System;
using System.Collections.Generic;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Owin.Routing;

namespace Tests
{
	[Ignore("NotImplemented")]
	[TestFixture]
	public class ReflectionRoutingTests
	{
		[Test]
		public async void Test()
		{
			var lm = new Mock<ILicenseManager>();
			lm.Setup(x => x.GetLicenses())
				.Returns(new[] {new LicenseInfo {SerialKey = "serialKey", Package = "package", Status = "activated", DaysLeft = 12}});
			lm.Setup(x => x.GetActivationKey(It.IsAny<string>())).Returns<string>(s => "123");

			using (var server = TestServer.Create(app => app.RegisterRoutes(_ => lm.Object)))
			{
				var s = await server.HttpClient.GetStringAsync("licenses");
				Assert.AreEqual("[{\"SerialKey\":\"serialKey\",\"Package\":\"package\",\"Status\":\"activated\",\"DaysLeft\":12}]", s);

				s = await server.HttpClient.GetStringAsync("licenses/abc/activationKey");
				Assert.AreEqual("\"123\"", s);
			}
		}
	}

	public interface ILicenseManager
	{
		[Route("GET", "licenses")]
		IEnumerable<LicenseInfo> GetLicenses();

		[Route("GET", "licenses/{serialKey}/activationKey")]
		string GetActivationKey(string serialKey);

		[Route("GET,POST", "licenses/{serialKey},{activationKey},{licenseKey}")]
		void AddLicense(string serialKey, string activationKey, string licenseKey);

		[Route("DELETE", "licenses/{serialKey}")]
		void RemoveLicense(string serialKey);

		[Route("GET", "licenses/{subjectId}/activate")]
		void Activate(Guid subjectId);

		[Route("GET", "licenses/{subjectId}/deactivate")]
		void Deactivate(Guid subjectId);

		[Route("GET", "licenses/{subjectId}/status")]
		string GetLicenseStatus(Guid subjectId);

		[Route("GET", "licenses/{subjectId}")]
		[Route("GET", "licenses/{subjectId}/info")]
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
