using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Owin.Routing
{
	using AppFunc = Func<IOwinContext, Func<Task>, Task>;
	using HandlerFunc = Func<IOwinContext, Task>;

	internal sealed class RouteData : Dictionary<string, string>
	{
		public RouteData() : base(StringComparer.InvariantCultureIgnoreCase)
		{
		}
	}

	/// <summary>
	/// Provides fluent API to register http method handlers.
	/// </summary>
	public sealed class RouteBuilder
	{
		private readonly Func<string, RouteData> _matcher;

		internal RouteBuilder(IAppBuilder app, string urlTemplate)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (string.IsNullOrWhiteSpace(urlTemplate)) throw new ArgumentNullException("urlTemplate");

			App = app;

			// TODO support wildcards when needed
			var template = (
				from s in urlTemplate.Trim('/').Split('/')
				// TODO support sinatra style '/resources/:id' templates
				let isVar = s.Length > 2 && s[0] == '{' && s[s.Length - 1] == '}'
				select isVar ? new {value = s.Substring(1, s.Length - 2), isVar = true} : new {value = s, isVar = false}
				).ToArray();

			_matcher = path =>
			{
				var segments = path.Split('/');
				if (segments.Length != template.Length) return null;

				var data = new RouteData();

				for (int i = 0; i < template.Length; i++)
				{
					var t = template[i];
					if (t.isVar)
					{
						data[t.value] = segments[i];
					}
					else if (!string.Equals(segments[i], t.value, StringComparison.InvariantCultureIgnoreCase))
					{
						return null;
					}
				}

				return data;
			};
		}

		private IAppBuilder App { get; set; }

		private RouteBuilder Register(string method, AppFunc handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			App.Use(async (ctx, next) =>
			{
				if (string.Equals(ctx.Request.Method, method, StringComparison.OrdinalIgnoreCase))
				{
					var path = ctx.Request.Path.Value.Trim('/');
					var data = _matcher(path);
					if (data != null)
					{
						ctx.Set(Keys.RouteData, data);
						await handler(ctx, next);
					}
					else
					{
						await next();
					}
				}
				else
				{
					await next();
				}
			});

			return this;
		}

		internal RouteBuilder Register(string method, HandlerFunc handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			return Register(method, (ctx, _) => handler(ctx));
		}

		/// <summary>
		/// Registers GET handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Get(HandlerFunc handler)
		{
			return Register(HttpMethod.Get, handler);
		}

		/// <summary>
		/// Registers GET handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Get(AppFunc handler)
		{
			return Register(HttpMethod.Get, handler);
		}

		/// <summary>
		/// Registers POST handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Post(HandlerFunc handler)
		{
			return Register(HttpMethod.Post, handler);
		}

		/// <summary>
		/// Registers POST handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Post(AppFunc handler)
		{
			return Register(HttpMethod.Post, handler);
		}

		/// <summary>
		/// Registers PUT handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Put(HandlerFunc handler)
		{
			return Register(HttpMethod.Put, handler);
		}

		/// <summary>
		/// Registers PUT handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Put(AppFunc handler)
		{
			return Register(HttpMethod.Put, handler);
		}

		/// <summary>
		/// Registers UPDATE handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		[Obsolete]
		public RouteBuilder Update(HandlerFunc handler)
		{
			return Register("UPDATE", handler);
		}

		/// <summary>
		/// Registers UPDATE handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		[Obsolete]
		public RouteBuilder Update(AppFunc handler)
		{
			return Register("UPDATE", handler);
		}

		/// <summary>
		/// Registers PATCH handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Patch(HandlerFunc handler)
		{
			return Register(HttpMethod.Patch, handler);
		}

		/// <summary>
		/// Registers PATCH handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Patch(AppFunc handler)
		{
			return Register(HttpMethod.Patch, handler);
		}

		/// <summary>
		/// Registers DELETE handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Delete(HandlerFunc handler)
		{
			return Register(HttpMethod.Delete, handler);
		}

		/// <summary>
		/// Registers DELETE handler.
		/// </summary>
		/// <param name="handler">The handler to register.</param>
		public RouteBuilder Delete(AppFunc handler)
		{
			return Register(HttpMethod.Delete, handler);
		}
	}
}
