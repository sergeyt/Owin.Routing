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
	/// Defines binding to request element.
	/// </summary>
	public abstract class BindingAttribute : Attribute
	{
		protected BindingAttribute(string name = null)
		{
			Name = string.IsNullOrEmpty(name) ? null : name;
		}

		/// <summary>
		/// Gets name of route/header/property to get value from.
		/// </summary>
		public string Name { get; private set; }

		internal abstract RequestElement Target { get; }
	}

	/// <summary>
	/// Defines binding to route variable.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class FromRouteAttribute : BindingAttribute
	{
		public FromRouteAttribute(string name = null) : base(name)
		{
		}

		internal override RequestElement Target
		{
			get { return RequestElement.Route; }
		}
	}

	/// <summary>
	/// Defines binding to query string parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class FromUriAttribute : BindingAttribute
	{
		public FromUriAttribute(string name = null) : base(name)
		{
		}

		internal override RequestElement Target
		{
			get {  return RequestElement.Query; }
		}
	}

	/// <summary>
	/// Defines binding to request header.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class FromHeaderAttribute : BindingAttribute
	{
		public FromHeaderAttribute(string name = null) : base(name)
		{
		}

		internal override RequestElement Target
		{
			get {  return RequestElement.Header; }
		}
	}

	/// <summary>
	/// Defines binding to request body property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class FromBodyAttribute : BindingAttribute
	{
		public FromBodyAttribute(string name = null) : base(name)
		{
		}

		internal override RequestElement Target
		{
			get { return RequestElement.Body; }
		}
	}
}
