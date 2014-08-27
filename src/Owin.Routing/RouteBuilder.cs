using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin;

namespace Owin.Routing
{
	using HandlerFunc = Func<IOwinContext, Task>;

	/// <summary>
	/// Provides fluent API to register http method handlers.
	/// </summary>
	public sealed class RouteBuilder : IRouteHandler
	{
		private readonly IDictionary<string, HandlerFunc> _verbs =
			new ConcurrentDictionary<string, HandlerFunc>(StringComparer.OrdinalIgnoreCase);

		internal RouteBuilder()
		{
		}

		internal bool HasHandler(string verb)
		{
			return _verbs.ContainsKey(verb);
		}

		internal async Task Invoke(IOwinContext context)
		{
			var handler = _verbs[context.Request.Method];
			await handler(context);
		}

		internal RouteBuilder Register(string method, HandlerFunc handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}

			if (_verbs.ContainsKey(method))
			{
				// already defined earlier
				return this;
			}

			_verbs[method] = handler;

			return this;
		}

		/// <summary>
		/// Registers GET handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Get(HandlerFunc handler)
		{
			return Register("GET", handler);
		}

		/// <summary>
		/// Registers POST handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Post(HandlerFunc handler)
		{
			return Register("POST", handler);
		}

		/// <summary>
		/// Registers PUT handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Put(HandlerFunc handler)
		{
			return Register("PUT", handler);
		}

		/// <summary>
		/// Registers UPDATE handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Update(HandlerFunc handler)
		{
			return Register("UPDATE", handler);
		}

		/// <summary>
		/// Registers PATCH handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Patch(HandlerFunc handler)
		{
			return Register("PATCH", handler);
		}

		/// <summary>
		/// Registers DELETE handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Delete(HandlerFunc handler)
		{
			return Register("DELETE", handler);
		}

		IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
		{
			throw new NotImplementedException();
		}
	}
}
