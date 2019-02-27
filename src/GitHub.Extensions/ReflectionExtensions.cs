using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GitHub.Extensions
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            Guard.ArgumentNotNull(assembly, nameof(assembly));

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static bool HasInterface(this Type type, Type targetInterface)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNull(targetInterface, nameof(targetInterface));

            if (targetInterface.IsAssignableFrom(type))
                return true;
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetInterface);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static string GetCustomAttributeValue<T>(this Assembly assembly, string propertyName) where T : Attribute
        {
            Guard.ArgumentNotNull(assembly, nameof(assembly));
            Guard.ArgumentNotEmptyString(propertyName, nameof(propertyName));

            if (assembly == null || string.IsNullOrEmpty(propertyName)) return string.Empty;

            object[] attributes = assembly.GetCustomAttributes(typeof(T), false);
            if (attributes.Length == 0) return string.Empty;

            var attribute = attributes[0] as T;
            if (attribute == null) return string.Empty;

            var propertyInfo = attribute.GetType().GetProperty(propertyName);
            if (propertyInfo == null) return string.Empty;

            var value = propertyInfo.GetValue(attribute, null);
            return value.ToString();
        }
    }
}
