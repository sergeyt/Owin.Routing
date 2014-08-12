using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Owin.Routing
{
	internal static class CustomAttributeProviderExtensions
	{
		public static T[] GetAttributes<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
		{
			return (T[])provider.GetCustomAttributes(typeof(T), inherit);
		}

		public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
		{
			var attrs = provider.GetAttributes<T>(inherit);
			return attrs.Length > 0 ? attrs[0] : null;
		}
	}

	internal static class ConvertExtensions
	{
		public static object ToType(this object value, Type type)
		{
			if (type == typeof(Guid))
			{
				return new Guid(Convert.ToString(value, CultureInfo.InvariantCulture));
			}

			var c = value as IConvertible;
			if (c != null)
			{
				return type.IsEnum ? value.ToEnum(type) : Convert.ChangeType(value, type);
			}

			var converter = TypeDescriptor.GetConverter(type);
			return converter.ConvertFrom(value);
		}

		public static T ToType<T>(this object value)
		{
			return (T)value.ToType(typeof(T));
		}

		public static object ToEnum(this object value, Type type)
		{
			if (value == null)
			{
				return Activator.CreateInstance(type);
			}

			var s = value as string;
			if (s != null)
			{
				var converter = TypeDescriptor.GetConverter(type);
				return converter.ConvertFrom(value);
			}
			return Enum.ToObject(type, value);
		}

		public static T ToEnum<T>(this object value)
		{
			return (T)value.ToEnum(typeof(T));
		}
	}
}
