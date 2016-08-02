using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Owin.Routing
{
	using HandlerFunc = Func<IOwinContext, Task>;

	internal sealed class RouteSegment
	{
		public string Name { get; set; }
		public bool IsVar { get; set; }
	}

	internal sealed class Route
	{
		public string Method { get; set; }
		public RouteSegment[] Segments { get; set; }
		public HandlerFunc Handler { get; set; }
	}

	internal sealed class RouteData : Dictionary<string, string>
	{
		public RouteData()
			: base(StringComparer.InvariantCultureIgnoreCase)
		{
		}
	}

	/// <summary>
	/// Provides fluent API to register http method handlers.
	/// </summary>
	internal sealed class RouteBuilderHelper
	{
		public static RouteData MatchData(RouteSegment[] template, string path)
		{
			var segments = path.Split('/');
			if (segments.Length != template.Length) return null;

			var data = new RouteData();

			for (var i = 0; i < template.Length; i++)
			{
				var t = template[i];
				if (t.IsVar)
				{
					data[t.Name] = segments[i];
				}
				else if (!string.Equals(segments[i], t.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					return null;
				}
			}

			return data;			
		}

		public static RouteSegment[] GetUrlTemplateSegments(string urlTemplate)
		{
			// TODO support wildcards when needed
			return (
				from s in urlTemplate.Trim('/').Split('/')
				// TODO support sinatra style '/resources/:id' templates
				let isVar = s.Length > 2 && s[0] == '{' && s[s.Length - 1] == '}'
				select isVar ? new RouteSegment {Name = s.Substring(1, s.Length - 2), IsVar = true} : new RouteSegment {Name = s}
				).ToArray();
		}
	}
}
