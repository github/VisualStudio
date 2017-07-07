using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using GitHub.Extensions;

namespace GitHub.Validation
{
    /// <summary>
    /// Used to conditionally run a set of validators. If this attribute is applied, and 
    /// the dependent property is false, no other validators run. This special case logic 
    /// occurs in ReactiveValidatableObject.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ValidateIfAttribute : ValidationAttribute
    {
        public ValidateIfAttribute(string dependentPropertyName)
        {
            Guard.ArgumentNotEmptyString(dependentPropertyName, nameof(dependentPropertyName));

            DependentPropertyName = dependentPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }

        public bool IsValidationRequired(ValidationContext validationContext)
        {
            Guard.ArgumentNotNull(validationContext, nameof(validationContext));

            var instance = validationContext.ObjectInstance;
            Debug.Assert(instance != null, "The ValidationContext does not allow null instances.");
            var property = instance.GetType().GetProperty(DependentPropertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (property == null || property.PropertyType != typeof(bool))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Could not find a boolean property named '{0}", DependentPropertyName));
            }

            return (bool)property.GetValue(instance, null);
        }

        public string DependentPropertyName { get; private set; }
    }
}
