using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Owin.Routing
{
	internal static class CustomAttributeProviderExtensions
	{
		public static T[] GetAttributes<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
		{
			return provider.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
		}

		public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
		{
			var attrs = provider.GetAttributes<T>(inherit);
			return attrs.Length > 0 ? attrs[0] : null;
		}

		public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
		{
			return provider.GetAttribute<T>(inherit) != null;
		}
	}

	internal static class DictionaryExtensions
	{
		public static T Get<T>(this IDictionary<string, object> dictionary, string key) where T : class
		{
			object value;
			return dictionary.TryGetValue(key, out value) ? value.ToType<T>() : null;
		}
	}

	internal static class ConvertExtensions
	{
		public static object ToType(this object value, Type type)
		{
			if (type.IsInstanceOfType(value))
			{
				return value;
			}

			if (type == typeof(Guid))
			{
				return new Guid(Convert.ToString(value, CultureInfo.InvariantCulture));
			}

			if (type.IsEnum)
			{
				return value.ToEnum(type);
			}

			var c = value as IConvertible;
			if (c != null)
			{
				return Convert.ChangeType(value, type);
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

	internal static class StreamExtensions
	{
		public static byte[] ToByteArray(this Stream stream)
		{
			if (stream.CanSeek)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}

			var buffer = new byte[16 * 1024];
			using (var ms = new MemoryStream())
			{
				int read;
				while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}
	}
}
