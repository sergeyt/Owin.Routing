using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Owin.Routing
{
	[DebuggerDisplay("{Method} {Source}/{Name}")]
	internal struct ParameterBinding
	{
		public enum DataSource
		{
			Route,
			Query,
			Header,
			Json
		}

		public readonly string Method;
		public readonly DataSource Source;
		public readonly string Name;

		public ParameterBinding(string method, DataSource source, string name)
		{
			Method = method;
			Source = source;
			Name = name;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}/{2}", Method, Source, Name);
		}
	}

	// GET route/name, POST json/name

	/// <summary>
	/// Specifies parameter bindings.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class ParameterAttribute : Attribute
	{
		private readonly IDictionary<string, ParameterBinding> _bindings = new Dictionary<string, ParameterBinding>(StringComparer.OrdinalIgnoreCase);
		
		public ParameterAttribute(params string[] bindings)
		{
			_bindings = new Dictionary<string, ParameterBinding>(StringComparer.OrdinalIgnoreCase);

			foreach (var spec in bindings)
			{
				if (string.IsNullOrWhiteSpace(spec)) continue;

				var parts = spec.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 0) continue;

				var method = parts.Length > 1 ? parts[0] : "*";
				var bs = parts.Length > 1 ? parts[1] : parts[0];
				parts = bs.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

				ParameterBinding.DataSource source;
				if (!Enum.TryParse(parts[0], true, out source)) continue;

				var name = parts.Length > 1 ? parts[1] : string.Empty;

				var binding = new ParameterBinding(method, source, name);
				if (!_bindings.ContainsKey(method))
				{
					_bindings.Add(method, binding);
				}
			}
		}

		internal ParameterBinding GetBinding(string method, string parameterName)
		{
			ParameterBinding binding;
			if (_bindings.TryGetValue(method, out binding))
			{
				return string.IsNullOrWhiteSpace(binding.Name)
					? new ParameterBinding(method, binding.Source, parameterName)
					: binding;
			}

			if (_bindings.TryGetValue("*", out binding))
			{
				return string.IsNullOrWhiteSpace(binding.Name)
					? new ParameterBinding(method, binding.Source, parameterName)
					: binding;
			}

			var source = method.Equals("POST", StringComparison.OrdinalIgnoreCase)
				? ParameterBinding.DataSource.Json
				: ParameterBinding.DataSource.Route;
			return new ParameterBinding(method, source, parameterName);
		}

#if NUNIT
		public override string ToString()
		{
			return string.Join(";", (from e in _bindings select e.Value.ToString()).ToArray());
		}
#endif
	}
}

#if NUNIT

namespace Owin.Routing.Tests
{
	using NUnit.Framework;

	[TestFixture]
	public class ParameterAttributeTests
	{
		[TestCase("GET route/param", Result = "GET Route/param")]
		[TestCase("GET query/param", Result = "GET Query/param")]
		[TestCase("GET header/param", Result = "GET Header/param")]
		[TestCase("GET json/param", Result = "GET Json/param")]

		[TestCase("GET route", Result = "GET Route/")]
		[TestCase("GET query", Result = "GET Query/")]
		[TestCase("GET header", Result = "GET Header/")]
		[TestCase("GET json", Result = "GET Json/")]

		[TestCase("route/param", Result = "* Route/param")]
		[TestCase("query/param", Result = "* Query/param")]
		[TestCase("header/param", Result = "* Header/param")]
		[TestCase("json/param", Result = "* Json/param")]
		
		[TestCase("route", Result = "* Route/")]
		[TestCase("query", Result = "* Query/")]
		[TestCase("header", Result = "* Header/")]
		[TestCase("json", Result = "* Json/")]
		
		[TestCase("invalid/param", Result = "")]
		public string Ctor(string binding)
		{
			return new ParameterAttribute(binding).ToString();
		}

		[TestCase("GET route/param", "GET", "p", Result = "GET Route/param")]
		[TestCase("GET route/param", "HEAD", "p", Result = "HEAD Route/p")]
		[TestCase("GET route/param", "POST", "p", Result = "POST Json/p")]
		public string GetBinding(string binding, string method, string name)
		{
			var attr = new ParameterAttribute(binding);
			return attr.GetBinding(method, name).ToString();
		}
	}
}

#endif
