using System;
using System.Diagnostics;
using System.Globalization;
using Splat;

namespace GitHub
{
    public static class Guard
    {
        /// <summary>
        /// Validates that the string is not empty.
        /// </summary>
        /// <param name="value"></param>
        public static void ArgumentNotEmptyString(string value, string name)
        {
            // We already know the value is not null because of NullGuard.Fody.
            if (!string.IsNullOrWhiteSpace(value)) return;

            string message = string.Format(CultureInfo.InvariantCulture, "The value for '{0}' must not be empty", name);
#if DEBUG
            if (!ModeDetector.InUnitTestRunner())
            {
                Debug.Fail(message);
            }
#endif
            throw new ArgumentException("String cannot be empty", name);
        }

        [AttributeUsage(AttributeTargets.Parameter)]
        internal sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}
