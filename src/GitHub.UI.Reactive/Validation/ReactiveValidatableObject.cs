using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using ReactiveUI;
using GitHub.Services;
using GitHub.Extensions;

#pragma warning disable CA1018 // Mark attributes with AttributeUsageAttribute

namespace GitHub.Validation
{
    public class ReactiveValidatableObject : ReactiveObject, IDataErrorInfo
    {
        static readonly ConcurrentDictionary<Type, Dictionary<string, ValidatedProperty>> typeValidatorsMap = new ConcurrentDictionary<Type, Dictionary<string, ValidatedProperty>>();
        const string isValidPropertyName = "IsValid";
        readonly IServiceProvider serviceProvider;
        readonly Dictionary<string, ValidatedProperty> validatedProperties;
        readonly Dictionary<string, bool> invalidProperties = new Dictionary<string, bool>();
        readonly Dictionary<string, bool> enabledProperties = new Dictionary<string, bool>();
        bool validationEnabled = true;

        public ReactiveValidatableObject(IGitHubServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            validatedProperties = typeValidatorsMap.GetOrAdd(GetType(), GetValidatedProperties);
            this.serviceProvider = serviceProvider; // This is allowed to be null.

            // NOTE: Until a property has been changed, we don't want to mark it as invalid 
            //       as far as IDataErrorInfo is concerned.
            Changed.Where(x => validatedProperties.ContainsKey(x.PropertyName))
                .Subscribe(x => enabledProperties[x.PropertyName] = true);
        }

        public string this[string propertyName]
        {
            get
            {
                Guard.ArgumentNotNull(propertyName, nameof(propertyName));

                if (!validationEnabled || !enabledProperties.ContainsKey(propertyName)) return null;

                string errorMessage = GetErrorMessage(propertyName);
                bool isValid = errorMessage == null;

                if (isValid && invalidProperties.ContainsKey(propertyName))
                {
                    invalidProperties.Remove(propertyName);
                    this.RaisePropertyChanged(isValidPropertyName); // TODO: See if I can make this more declarative with a reactive dictionary, if there is one.
                }
                else if (!isValid)
                {
                    invalidProperties[propertyName] = true;
                    this.RaisePropertyChanged(isValidPropertyName); // TODO: See if I can make this more declarative with a reactive dictionary, if there is one.
                }

                return errorMessage;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Need to ask the author why this is not implemented and to add a message as such to the exception.")]
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsValid
        {
            get { return !invalidProperties.Any(); }
        }

        public bool Validate()
        {
            bool wasValid = IsValid;
            var unvalidated = EnableValidationForUnvalidatedProperties();

            unvalidated.ForEach(TriggerValidationForProperty);
            if (IsValid != wasValid)
            {
                this.RaisePropertyChanged(isValidPropertyName);
            }
            return IsValid;
        }

        public void ResetValidation()
        {
            try
            {
                validationEnabled = false;
                TriggerValidationForAllProperties(); // Tell the UI every property is now valid.
                invalidProperties.Clear();
                enabledProperties.Clear();
                foreach (var v in validatedProperties.Values)
                    v.Reset();
                this.RaisePropertyChanged(isValidPropertyName);
            }
            finally
            {
                validationEnabled = true;
            }
        }

        // Enables validation for any properties that have not yet been validated and 
        // returns that set of properties.
        IEnumerable<string> EnableValidationForUnvalidatedProperties()
        {
            return validatedProperties.Keys.Where(key => !enabledProperties.ContainsKey(key))
                .Do(propertyName => enabledProperties[propertyName] = true);
        }

        // Made this its own method because it's not clear that the way to trigger validation is to 
        // raise a property changed event.
        void TriggerValidationForProperty(string propertyName)
        {
            Guard.ArgumentNotNull(propertyName, nameof(propertyName));

            this.RaisePropertyChanged(propertyName);
        }

        void TriggerValidationForAllProperties()
        {
            validatedProperties.Keys.ForEach(TriggerValidationForProperty);
        }


        string GetErrorMessage(string propertyName)
        {
            Guard.ArgumentNotEmptyString(propertyName, nameof(propertyName));

            ValidatedProperty validatedProperty;
            if (!validatedProperties.TryGetValue(propertyName, out validatedProperty))
                return null; // TODO: This would be a good place to do default data type validation as the need arises.

            var validationResult = validatedProperty.GetFirstValidationError(this, serviceProvider);
            return validationResult == ValidationResult.Success ? null : validationResult.ErrorMessage;
        }

        public void SetErrorMessage(string propertyName, string errorMessage)
        {
            Guard.ArgumentNotEmptyString(propertyName, nameof(propertyName));
            Guard.ArgumentNotEmptyString(errorMessage, nameof(errorMessage));

            ValidatedProperty validatedProperty;
            if (!validatedProperties.TryGetValue(propertyName, out validatedProperty))
                return;

            validatedProperty.AddValidator(new SetErrorValidator(errorMessage, validatedProperty.Property.GetValue(this, null)));
            TriggerValidationForProperty(propertyName);
        }

        static Dictionary<string, ValidatedProperty> GetValidatedProperties(Type type)
        {
            Guard.ArgumentNotNull(type, nameof(type));

            return (from property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    let validated = new ValidatedProperty(property)
                    where validated.Validators.Any()
                    select validated).ToDictionary(p => p.Property.Name, p => p);
        }

        class ValidatedProperty
        {
            readonly IList<ValidationAttribute> validators;

            public ValidatedProperty(PropertyInfo property)
            {
                Guard.ArgumentNotNull(property, nameof(property));

                Property = property;
                validators = property.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>().ToList();
                Validators = validators.Where(v => !(v is ValidateIfAttribute));
                ConditionalValidation = validators.FirstOrDefault(v => v is ValidateIfAttribute) as ValidateIfAttribute;
            }

            public void AddValidator(ValidationAttribute validator)
            {
                Guard.ArgumentNotNull(validator, nameof(validator));

                validators.Add(validator);
            }

            public ValidationResult GetFirstValidationError(object instance, IServiceProvider serviceProvider)
            {
                Guard.ArgumentNotNull(instance, nameof(instance));
                Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

                var validationContext = new ValidationContext(instance, serviceProvider, null) { MemberName = Property.Name };

                if (ConditionalValidation != null && !ConditionalValidation.IsValidationRequired(validationContext))
                {
                    return ValidationResult.Success;
                }

                var value = Property.GetValue(instance, null);
                return (from validator in Validators
                        let r = validator.GetValidationResult(value, validationContext)
                        where r != null
                        select r).FirstOrDefault();
            }

            public void Reset()
            {
                var setErrorValidators = validators.Where(x => x is SetErrorValidator).ToList();
                setErrorValidators.ForEach(x => validators.Remove(x));
            }

            public PropertyInfo Property { get; private set; }
            public IEnumerable<ValidationAttribute> Validators { get; private set; }
            ValidateIfAttribute ConditionalValidation { get; set; }
        }

        sealed class SetErrorValidator : ValidationAttribute
        {
            readonly string errorMessage;
            readonly object originalValue;

            public SetErrorValidator(string errorMessage, object originalValue)
            {
                Guard.ArgumentNotEmptyString(errorMessage, nameof(errorMessage));
                Guard.ArgumentNotNull(originalValue, nameof(originalValue));

                this.errorMessage = errorMessage;
                this.originalValue = originalValue;
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Guard.ArgumentNotNull(value, nameof(value));
                Guard.ArgumentNotNull(validationContext, nameof(validationContext));

                if (originalValue.Equals(value))
                    return new ValidationResult(errorMessage);

                return ValidationResult.Success;
            }
        }
    }
}