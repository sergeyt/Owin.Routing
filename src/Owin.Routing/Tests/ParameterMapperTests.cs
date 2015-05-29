# if NUNIT

using System;
using System.IO;
using System.Text;

using Microsoft.Owin;
using Newtonsoft.Json.Linq;

using Moq;
using NUnit.Framework;

namespace Owin.Routing.Tests
{
	[TestFixture]
	public class ParameterMapperTests
	{
		private Stream _jsonStream;
		private Stream _jsonArrayStream;

		[SetUp]
		public void Setup()
		{
			_jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(JObject.FromObject(new {Name = "test"}).ToString()));
			_jsonArrayStream = new MemoryStream(Encoding.UTF8.GetBytes(JArray.FromObject(new[] {new {Name = "test"}}).ToString()));
		}

		[Test]
		public void MapContext()
		{
			var ctx = Mock.Of<IOwinContext>();
			var mapper = Build("MapContext");
			var args = mapper(ctx);
			Assert.AreSame(ctx, args[0]);
		}

		[Test]
		public void MapRequest()
		{
			var req = Mock.Of<IOwinRequest>();
			var ctx = Mock.Of<IOwinContext>(x => x.Request == req);
			var mapper = Build("MapRequest");
			var args = mapper(ctx);
			Assert.AreSame(req, args[0]);
		}

		[Test]
		public void MapResponse()
		{
			var res = Mock.Of<IOwinResponse>();
			var ctx = Mock.Of<IOwinContext>(x => x.Response == res);
			var mapper = Build("MapResponse");
			var args = mapper(ctx);
			Assert.AreSame(res, args[0]);
		}

		[Test]
		public void MapStream()
		{
			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Request.Body).Returns(_jsonStream);

			var mapper = Build("MapStream");
			var args = mapper(ctx.Object);
			Assert.IsInstanceOf<Stream>(args[0]);

			ctx.Verify(x => x.Request.Body, Times.Once);
		}

		[Test]
		public void MapByteArray()
		{
			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<byte[]>(Keys.RequestBytes)).Returns((byte[]) null);
			ctx.Setup(x => x.Request.Body).Returns(_jsonStream);

			var mapper = Build("MapByteArray");
			var args = mapper(ctx.Object);
			Assert.IsInstanceOf<byte[]>(args[0]);

			ctx.Verify(x => x.Get<byte[]>(Keys.RequestBytes), Times.Once);
			ctx.Verify(x => x.Request.Body, Times.Once);
		}

		[Test]
		public void MapJToken()
		{
			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<JToken>(Keys.JsonBody)).Returns((JToken)null);
			ctx.Setup(x => x.Request.Body).Returns(_jsonStream);

			var mapper = Build("MapJToken");
			var args = mapper(ctx.Object);
			Assert.IsInstanceOf<JToken>(args[0]);

			ctx.Verify(x => x.Get<JToken>(Keys.JsonBody), Times.Once);
			ctx.Verify(x => x.Request.Body, Times.Once);
		}

		[Test]
		public void MapJObject()
		{
			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<JToken>(Keys.JsonBody)).Returns((JToken)null);
			ctx.Setup(x => x.Request.Body).Returns(_jsonStream);

			var mapper = Build("MapJObject");
			var args = mapper(ctx.Object);
			Assert.IsInstanceOf<JObject>(args[0]);

			ctx.Verify(x => x.Get<JToken>(Keys.JsonBody), Times.Once);
			ctx.Verify(x => x.Request.Body, Times.Once);
		}

		[Test]
		public void MapJArray()
		{
			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<JToken>(Keys.JsonBody)).Returns((JToken)null);
			ctx.Setup(x => x.Request.Body).Returns(_jsonArrayStream);

			var mapper = Build("MapJArray");
			var args = mapper(ctx.Object);
			Assert.IsInstanceOf<JArray>(args[0]);

			ctx.Verify(x => x.Get<JToken>(Keys.JsonBody), Times.Once);
			ctx.Verify(x => x.Request.Body, Times.Once);
		}

		[Test]
		public void MapRoute()
		{
			var data = new RouteData {{"value", "test"}};

			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<RouteData>(Keys.RouteData)).Returns(data);
			ctx.Setup(x => x.Request.Method).Returns("GET");
			
			var mapper = Build("MapRoute");
			var args = mapper(ctx.Object);
			Assert.AreEqual("test", args[0]);
			
			ctx.Verify(x => x.Get<RouteData>(Keys.RouteData), Times.Once);
			ctx.Verify(x => x.Request.Method, Times.Once);
		}

		[Test]
		public void MapRouteDefault()
		{
			var data = new RouteData {{"value", "test"}};

			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<RouteData>(Keys.RouteData)).Returns(data);

			var mapper = Build("MapRouteDefault");
			var args = mapper(ctx.Object);
			Assert.AreEqual("test", args[0]);

			ctx.Verify(x => x.Get<RouteData>(Keys.RouteData), Times.Once);
		}

		[Test]
		public void MapRouteArray()
		{
			var data = new RouteData {{"values", "a,b,c"}};

			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<RouteData>(Keys.RouteData)).Returns(data);

			var mapper = Build("MapRouteArray");
			var args = mapper(ctx.Object);
			var array = args[0] as string[];
			CollectionAssert.AreEqual(new [] { "a","b", "c"}, array);

			ctx.Verify(x => x.Get<RouteData>(Keys.RouteData), Times.Once);
		}

		[Test]
		public void MapRouteUnescape()
		{
			const string value = "a,b,c";
			var data = new RouteData { Values = { { "value", Uri.EscapeDataString(value) } } };

			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<RouteData>(Keys.RouteData)).Returns(data);

			var mapper = Build("MapRouteDefault");
			var args = mapper(ctx.Object);
			Assert.AreEqual(value, args[0]);
		}

		[Test]
		public void MapQuery()
		{
			var query = Mock.Of<IReadableStringCollection>(c => c.Get("value") == "test");

			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Request.Method).Returns("GET");
			ctx.Setup(x => x.Request.Query).Returns(query);

			var mapper = Build("MapQuery");
			var args = mapper(ctx.Object);
			Assert.AreEqual("test", args[0]);

			ctx.Verify(x => x.Request.Method, Times.Once);
			ctx.Verify(x => x.Request.Query, Times.Once);
		}

		[Test]
		public void MapQueryUnescape()
		{
			const string value = "a,b,c";
			var query = Mock.Of<IReadableStringCollection>(c => c.Get("value") == Uri.EscapeDataString(value));

			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Request.Method).Returns("GET");
			ctx.Setup(x => x.Request.Query).Returns(query);

			var mapper = Build("MapQuery");
			var args = mapper(ctx.Object);
			Assert.AreEqual(value, args[0]);
		}

		[Test]
		public void MapHeader()
		{
			var headers = Mock.Of<IHeaderDictionary>(c => c.Get("value") == "test");

			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Request.Method).Returns("GET");
			ctx.Setup(x => x.Request.Headers).Returns(headers);

			var mapper = Build("MapHeader");
			var args = mapper(ctx.Object);
			Assert.AreEqual("test", args[0]);

			ctx.Verify(x => x.Request.Method, Times.Once);
			ctx.Verify(x => x.Request.Headers, Times.Once);
		}

		[Test]
		public void MapJsonValue()
		{
			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Request.Method).Returns("GET");
			ctx.Setup(x => x.Request.Body).Returns(_jsonStream);

			var mapper = Build("MapJsonValue");
			var args = mapper(ctx.Object);
			Assert.AreEqual("test", args[0]);

			ctx.Verify(x => x.Request.Method, Times.Once);
			ctx.Verify(x => x.Request.Body, Times.Once);
		}

		[Test]
		public void MapJsonValueLowCase()
		{
			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Request.Method).Returns("GET");
			ctx.Setup(x => x.Request.Body).Returns(_jsonStream);

			var mapper = Build("MapJsonValueLowCase");
			var args = mapper(ctx.Object);
			Assert.AreEqual("test", args[0]);

			ctx.Verify(x => x.Request.Method, Times.Once);
			ctx.Verify(x => x.Request.Body, Times.Once);
		}

		[Test]
		public void MapOptions()
		{
			var data = new RouteData {{"Name", "test"}};

			var ctx = new Mock<IOwinContext>();
			ctx.Setup(x => x.Get<RouteData>(Keys.RouteData)).Returns(data);
			ctx.Setup(x => x.Get<JToken>(Keys.JsonBody)).Returns(JObject.FromObject(new {Name = "test"}));

			var mapper = Build("MapOptions");
			var args = mapper(ctx.Object);
			var options = args[0] as Options;
			Assert.IsNotNull(options);
			Assert.AreEqual("test", options.NameFromRoute);
			Assert.AreEqual("test", options.NameFromRoute2);
			Assert.AreEqual("test", options.NameFromBody);
			Assert.AreEqual("test", options.NameFromBody2);

			ctx.Verify(x => x.Get<RouteData>(Keys.RouteData), Times.AtLeastOnce);
			ctx.Verify(x => x.Get<JToken>(Keys.JsonBody), Times.AtLeastOnce);
		}

		private static Func<IOwinContext, object[]> Build(string methodName)
		{
			var method = typeof(Entity).GetMethod(methodName);
			return ParameterMapper.Build(method);
		}

		private sealed class Entity
		{
			public void MapContext(IOwinContext context) { }
			public void MapRequest(IOwinRequest request) { }
			public void MapResponse(IOwinResponse response) { }
			public void MapStream(Stream input) { }
			public void MapByteArray(byte[] input) { }
			public void MapJToken(JToken input) { }
			public void MapJObject(JObject input) { }
			public void MapJArray(JArray input) { }

			public void MapRoute([Bindings("route")] string value) { }
			public void MapRouteDefault(string value) { }
			public void MapRouteArray(string[] values) { }
			public void MapQuery([Bindings("query")] string value) { }
			public void MapHeader([Bindings("header")] string value) { }
			public void MapJsonValue([Bindings("body.Name")] string value) { }
			public void MapJsonValueLowCase([Bindings("body.name")] string value) { }

			public void MapOptions(Options options) { }
		}

		private sealed class Options
		{
			[Map(RequestElement.Route, "Name")]
			public string NameFromRoute { get; set; }

			[Map(RequestElement.Route, "Name")]
			public string NameFromRoute2;

			[Map(RequestElement.Body, "Name")]
			public string NameFromBody { get; set; }

			[Map(RequestElement.Body, "Name")]
			public string NameFromBody2;
		}
	}
}

#endif
