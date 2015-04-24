using System;

namespace Owin.Routing
{
	internal static class HttpMethod
	{
		public const string Get = "GET";
		public const string Head = "HEAD";
		public const string Options = "OPTIONS";
		public const string Post = "POST";
		public const string Patch = "PATCH";
		public const string Put = "PUT";
		public const string Delete = "DELETE";
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class HttpMethodAttribute : Attribute
	{
		protected HttpMethodAttribute(string method)
		{
			Method = method;
		}

		public string Method { get; private set; }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class HttpGetAttribute : HttpMethodAttribute
	{
		public HttpGetAttribute() : base(HttpMethod.Get) { }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class HttpHeadAttribute : HttpMethodAttribute
	{
		public HttpHeadAttribute() : base(HttpMethod.Head) { }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class HttpOptionsAttribute : HttpMethodAttribute
	{
		public HttpOptionsAttribute() : base(HttpMethod.Options) { }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class HttpPostAttribute : HttpMethodAttribute
	{
		public HttpPostAttribute() : base(HttpMethod.Post) { }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class HttpPatchAttribute : HttpMethodAttribute
	{
		public HttpPatchAttribute() : base(HttpMethod.Patch) { }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class HttpPutAttribute : HttpMethodAttribute
	{
		public HttpPutAttribute() : base(HttpMethod.Put) { }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class HttpDeleteAttribute : HttpMethodAttribute
	{
		public HttpDeleteAttribute() : base(HttpMethod.Delete) { }
	}
}