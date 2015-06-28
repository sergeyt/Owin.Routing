using System;

namespace Owin.Routing
{
	/// <summary>
	/// Defines method to call when parameter mapping failed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ErrorHandlerAttribute : Attribute
	{
	}
}