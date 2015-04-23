using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Owin;

namespace Owin.Routing
{
	/// <summary>
	/// Implements <see cref="HttpContextBase"/> using <see cref="IOwinContext"/>.
	/// This is partial implementation that should be enough for routing.
	/// </summary>
	internal sealed class HttpContextImpl : HttpContextBase
	{
		private readonly HttpRequestImpl _request;

		public HttpContextImpl(IOwinContext context)
		{
			_request = new HttpRequestImpl(context.Request);
		}

		public override HttpRequestBase Request
		{
			get { return _request; }
		}
	}

	internal sealed class HttpRequestImpl : HttpRequestBase
	{
		private readonly IOwinRequest _request;
		private NameValueCollection _headers;
		private NameValueCollection _queryString;

		public HttpRequestImpl(IOwinRequest request)
		{
			_request = request;
		}

		public override string HttpMethod { get { return _request.Method; } }
		public override Uri Url { get { return _request.Uri; } }
		public override string AppRelativeCurrentExecutionFilePath { get { return "~/"; } }
		public override string Path { get { return _request.Path.ToString(); } }
		public override string PathInfo { get { return Path.TrimStart('/'); } }
		public override string RawUrl { get { return _request.Uri.ToString(); } }
		public override bool IsLocal { get { return false; } }
		public override bool IsSecureConnection { get { return false; } }
		public override void ValidateInput() { }
		public override bool IsAuthenticated { get { return true; } }

		public override NameValueCollection Headers
		{
			get { return _headers ?? (_headers = _request.Headers.ToNameValueCollection()); }
		}

		public override NameValueCollection QueryString
		{
			get { return _queryString ?? (_queryString = _request.Query.ToNameValueCollection()); }
		}

		public override string ContentType
		{
			get { return _request.ContentType; }
			set { _request.ContentType = value; }
		}

		public override string ApplicationPath
		{
			get { return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}
	}

	internal static class ReadableStringCollectionExtensions
	{
		public static NameValueCollection ToNameValueCollection(this IReadableStringCollection collection)
		{
			var result = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
			var pairs = from p in collection select new KeyValuePair<string, string>(p.Key, collection.Get(p.Key));
			foreach (var h in pairs)
			{
				result.Add(h.Key, string.Join(",", h.Value));
			}
			return result;
		}
	}
}
