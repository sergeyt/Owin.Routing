using System;
using System.Linq;

namespace Owin.Routing
{
	[AttributeUsage(AttributeTargets.Class
	                | AttributeTargets.Interface
	                | AttributeTargets.Method
	                | AttributeTargets.Property
	                | AttributeTargets.Field
		, AllowMultiple = true)]
	public sealed class RouteAttribute : Attribute
	{
		public RouteAttribute(string methods, string url)
		{
			if (string.IsNullOrEmpty(methods)) throw new ArgumentNullException("methods");
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");

			Methods = (from s in methods.Split(',') let ts = s.Trim() where ts.Length > 0 select ts).ToArray();
			Url = url;
		}

		/// <summary>
		/// HTTP method.
		/// </summary>
		public string[] Methods { get; private set; }

		/// <summary>
		/// Url template.
		/// </summary>
		public string Url { get; private set; }
	}
}
