using System;

namespace Owin.Routing
{
	/// <summary>
	/// Apply this attribute to method to specify that this method is REST handler for given URL pattern.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class RouteAttribute : Attribute
	{
		public RouteAttribute(string template)
		{
			if (string.IsNullOrWhiteSpace(template)) throw new ArgumentNullException("template");

			Template = template;
		}

		/// <summary>
		/// Specifies route name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Specifies URL pattern.
		/// </summary>
		public string Template { get; private set; }
	}

	/// <summary>
	/// Annotates an class with REST handlers with a route prefix that applies to actions that have any <see cref="RouteAttribute"/>s on them.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class RoutePrefixAttribute : Attribute
	{
		public RoutePrefixAttribute(string prefix)
		{
			if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentNullException("prefix");

			Prefix = prefix;
		}

		public string Prefix { get; private set; }
	}
}
