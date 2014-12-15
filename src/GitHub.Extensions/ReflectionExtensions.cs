using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GitHub.Extensions
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
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
            if (targetInterface.IsAssignableFrom(type))
                return true;
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetInterface);
        }
    }
}
