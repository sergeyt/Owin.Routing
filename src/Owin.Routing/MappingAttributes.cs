using System;

namespace Owin.Routing
{
	/// <summary>
	/// Binds parameter to JSON body of HTTP request.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class MapJsonAttribute : Attribute
	{
	}

	/// <summary>
	/// Defines mapping to specified <see cref="RequestElement"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class MapAttribute : Attribute
	{
		public MapAttribute(RequestElement target, string name = null)
		{
			Target = target;
			Name = name;
		}

		/// <summary>
		/// Specifies request element to bind member to.
		/// </summary>
		public RequestElement Target { get; private set; }

		/// <summary>
		/// Specifies parameter name of request element.
		/// </summary>
		public string Name { get; private set; }
	}

	/// <summary>
	/// Defines method to call when parameter mapping failed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ErrorHandlerAttribute : Attribute
	{
	}
}
