#if NUNIT
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Owin.Routing.Tests
{
	[TestFixture]
	internal class CustomIntJsonConverterTests
	{
		private JsonSerializer _serializer;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_serializer = new JsonSerializer();
			_serializer.Converters.Add(new CustomIntJsonConverter());
		}

		[TestCase("{Value:3}", Result = 3)]
		[TestCase("{Value:0}", Result = 0)]
		[TestCase("{Value:-2147483648}", Result = int.MinValue)]
		[TestCase("{Value:2147483647}", Result = int.MaxValue)]
		[TestCase("{Value:null}", ExpectedException = typeof(NullReferenceException))]
		[TestCase("{Value:3.14}", ExpectedException = typeof(FormatException))]
		public int ReadIntValue(string json)
		{
			var obj = JObject.Parse(json);
			var item = obj.ToObject<IntItem>(_serializer);
			return item.Value;
		}

		[TestCase("{Value:3}", Result = 3)]
		[TestCase("{Value:0}", Result = 0)]
		[TestCase("{Value:-2147483648}", Result = int.MinValue)]
		[TestCase("{Value:2147483647}", Result = int.MaxValue)]
		[TestCase("{Value:null}", Result = null)]
		[TestCase("{Value:3.14}", ExpectedException = typeof(FormatException))]
		public int? ReadNullableIntValue(string json)
		{
			var obj = JObject.Parse(json);
			var item = obj.ToObject<NullableIntItem>(_serializer);
			return item.Value;
		}

		internal class IntItem
		{
			public int Value { get; set; }
		}

		internal class NullableIntItem
		{
			public int? Value { get; set; }
		}
	}
}

#endif
