using System;

namespace Owin.Routing
{
	/// <summary>
	/// Mark your method with this attribute to specify route (URL pattern and HTTML methods) to be registered in OWIN pipeline.
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
}
