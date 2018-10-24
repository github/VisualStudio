using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Primitives;
using ReactiveUI;

namespace GitHub.Validation
{
    public class ReactivePropertyValidationResult
    {
        /// <summary>
        /// Describes if the property passes validation
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Describes which state we are in - Valid, Not Validated, or Invalid
        /// </summary>
        public ValidationStatus Status { get; private set; }

        /// <summary>
        /// An error message to display
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Describes if we should show this error in the UI
        /// We only show errors which have been marked specifically as Invalid
        /// and we do not show errors for inputs which have not yet been validated. 
        /// </summary>
        public bool DisplayValidationError { get; private set; }

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It is immutable")]
        public static readonly ReactivePropertyValidationResult Success = new ReactivePropertyValidationResult(ValidationStatus.Valid);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It is immutable")]
        public static readonly ReactivePropertyValidationResult Unvalidated = new ReactivePropertyValidationResult();

        public ReactivePropertyValidationResult() : this(ValidationStatus.Unvalidated, "")
        {
        }

        public ReactivePropertyValidationResult(ValidationStatus validationStatus) : this(validationStatus, "")
        {
        }

        public ReactivePropertyValidationResult(ValidationStatus validationStatus, string message)
        {
            Status = validationStatus;
            IsValid = validationStatus == ValidationStatus.Valid;
            DisplayValidationError = validationStatus == ValidationStatus.Invalid;
            Message = message;
        }
    }

    public enum ValidationStatus
    {
        Unvalidated = 0,
        Invalid = 1,
        Valid = 2,
    }

    public abstract class ReactivePropertyValidator : ReactiveObject, IDisposable
    {
        public static ReactivePropertyValidator<TProp> For<TObj, TProp>(TObj This, Expression<Func<TObj, TProp>> property)
        {
            return new ReactivePropertyValidator<TObj, TProp>(This, property);
        }

        public static ReactivePropertyValidator<TProp> ForObservable<TProp>(IObservable<TProp> propertyObservable)
        {
            return new ReactivePropertyValidator<TProp>(propertyObservable);
        }

        public abstract ReactivePropertyValidationResult ValidationResult { get; }

        public abstract bool IsValidating { get; }

        protected ReactivePropertyValidator()
        {
        }

        public abstract Task<ReactivePropertyValidationResult> ExecuteAsync();

        public abstract Task ResetAsync();

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class ReactivePropertyValidator<TProp> : ReactivePropertyValidator
    {
        readonly ReactiveCommand<ValidationParameter, ReactivePropertyValidationResult> validateCommand;
        ValidationParameter currentValidationParameter;
        ObservableAsPropertyHelper<ReactivePropertyValidationResult> validationResult;

        public override ReactivePropertyValidationResult ValidationResult
        {
            get { return validationResult.Value; }
        }

        public override Task<ReactivePropertyValidationResult> ExecuteAsync()
        {
            return validateCommand.Execute(currentValidationParameter).ToTask();
        }

        public override Task ResetAsync()
        {
            return validateCommand.Execute(new ValidationParameter { RequiresReset = true })
                .Select(_ => Unit.Default)
                .ToTask();
        }

        readonly List<Func<TProp, IObservable<ReactivePropertyValidationResult>>> validators =
            new List<Func<TProp, IObservable<ReactivePropertyValidationResult>>>();

        readonly ObservableAsPropertyHelper<bool> _isValidating;
        public override bool IsValidating
        {
            get { return _isValidating.Value; }
        }

        public ReactivePropertyValidator(IObservable<TProp> propertyChangeSignal)
        {
            validateCommand = ReactiveCommand.CreateFromObservable((ValidationParameter param) =>
            {
                var validationParams = param ?? new ValidationParameter();

                if (validationParams.RequiresReset)
                {
                    return Observable.Return(ReactivePropertyValidationResult.Unvalidated);
                }

                TProp value = validationParams.PropertyValue;

                var currentValidators = validators.ToList();

                // HEAR YE, HEAR YE

                // This .ToList() is here to ignore changes to the validator collection,
                // and thus avoid fantastically vague exceptions about 
                // "Collection was modified, enumeration operation may not execute"
                // bubbling up to tear the application down

                // Thus, the collection will be correct when the command executes,
                // which should be fine until we need to do more complex validation

                if (!currentValidators.Any())
                    return Observable.Return(ReactivePropertyValidationResult.Unvalidated);

                return currentValidators.ToObservable()
                    .SelectMany(v => v(value))
                    .FirstOrDefaultAsync(x => x.Status == ValidationStatus.Invalid)
                    .Select(x => x == null ? ReactivePropertyValidationResult.Success : x);
            });

            _isValidating = validateCommand.IsExecuting.ToProperty(this, x => x.IsValidating);

            validationResult = validateCommand.ToProperty(this, x => x.ValidationResult);
            propertyChangeSignal
                .Select(x => new ValidationParameter { PropertyValue = x, RequiresReset = false })
                .Do(validationParameter => currentValidationParameter = validationParameter)
                .Subscribe(validationParameter => validateCommand.Execute(validationParameter).Subscribe());
        }

        public ReactivePropertyValidator<TProp> IfTrue(Func<TProp, bool> predicate, string errorMessage)
        {
            return Add(predicate, errorMessage);
        }

        public ReactivePropertyValidator<TProp> IfFalse(Func<TProp, bool> predicate, string errorMessage)
        {
            return Add(x => !predicate(x), errorMessage);
        }

        ReactivePropertyValidator<TProp> Add(Func<TProp, bool> predicate, string errorMessage)
        {
            return Add(x => predicate(x) ? errorMessage : null);
        }

        public ReactivePropertyValidator<TProp> Add(Func<TProp, string> predicateWithMessage)
        {
            validators.Add(value => Observable.Defer(() => Observable.Return(Validate(value, predicateWithMessage))));
            return this;
        }

        public ReactivePropertyValidator<TProp> IfTrueAsync(Func<TProp, IObservable<bool>> predicate, string errorMessage)
        {
            AddAsync(x => predicate(x).Select(result => result ? errorMessage : null));
            return this;
        }

        public ReactivePropertyValidator<TProp> IfFalseAsync(Func<TProp, IObservable<bool>> predicate, string errorMessage)
        {
            AddAsync(x => predicate(x).Select(result => result ? null : errorMessage));
            return this;
        }

        public ReactivePropertyValidator<TProp> AddAsync(Func<TProp, IObservable<string>> predicateWithMessage)
        {
            validators.Add(value => Observable.Defer(() =>
            {
                return predicateWithMessage(value)
                    .Select(result => String.IsNullOrEmpty(result)
                        ? ReactivePropertyValidationResult.Success
                        : new ReactivePropertyValidationResult(ValidationStatus.Invalid, result));

            }));

            return this;
        }

        static ReactivePropertyValidationResult Validate(TProp value, Func<TProp, string> predicateWithMessage)
        {
            var result = predicateWithMessage(value);

            if (String.IsNullOrEmpty(result))
                return ReactivePropertyValidationResult.Success;

            return new ReactivePropertyValidationResult(ValidationStatus.Invalid, result);
        }

        class ValidationParameter
        {
            public TProp PropertyValue { get; set; }
            public bool RequiresReset { get; set; }
        }
    }

    public class ReactivePropertyValidator<TObj, TProp> : ReactivePropertyValidator<TProp>
    {
        protected ReactivePropertyValidator() : base(Observable.Empty<TProp>())
        {
        }

        public ReactivePropertyValidator(TObj This, Expression<Func<TObj, TProp>> property)
            : base(This.WhenAny(property, x => x.Value))
        { }
    }

    public static class ReactivePropertyValidatorExtensions
    {
        public static ReactivePropertyValidator<string> IfMatch(this ReactivePropertyValidator<string> This, string pattern, string errorMessage)
        {
            var regex = new Regex(pattern);

            return This.IfTrue(regex.IsMatch, errorMessage);
        }

        public static ReactivePropertyValidator<string> IfNotMatch(this ReactivePropertyValidator<string> This, string pattern, string errorMessage)
        {
            var regex = new Regex(pattern);

            return This.IfFalse(regex.IsMatch, errorMessage);
        }

        public static ReactivePropertyValidator<string> IfNullOrEmpty(this ReactivePropertyValidator<string> This, string errorMessage)
        {
            return This.IfTrue(String.IsNullOrEmpty, errorMessage);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.UriBuilder",
            Justification = "We're using UriBuilder to validate the URL because Uri.TryCreate fails if no scheme specified.")]
        public static ReactivePropertyValidator<string> IfNotUri(this ReactivePropertyValidator<string> This, string errorMessage)
        {
            return This.IfFalse(s =>
            {
                try
                {
                    new UriBuilder(s);
                    return true;
                }
                catch
                {
                    return false;
                }
            }, errorMessage);
        }

        public static ReactivePropertyValidator<string> IfGitHubDotComHost(
            this ReactivePropertyValidator<string> This,
            string errorMessage)
        {
            return This.IfTrue(s =>
            {
                try
                {
                    var hostAddress = HostAddress.Create(s);
                    return hostAddress == HostAddress.GitHubDotComHostAddress;
                }
                catch (Exception)
                {
                    // A previous validation should probably have caught this.
                    return false;
                }
            }, errorMessage);
        }

        public static ReactivePropertyValidator<string> IfContainsInvalidPathChars(this ReactivePropertyValidator<string> This, string errorMessage)
        {
            return This.IfTrue(str =>
            {
                // easiest check to make
                if (str.ContainsAny(Path.GetInvalidPathChars()))
                {
                    return true;
                }

                string driveLetter;

                try
                {
                    // if for whatever reason you don't have an absolute path
                    // hopefully you've remembered to use `IfPathNotRooted`
                    // in your validator
                    driveLetter = Path.GetPathRoot(str);
                }
                catch (PathTooLongException)
                {
                    // when you pass in a path that is too long
                    // you're gonna have a bad time:
                    // - 260 characters for full path
                    // - 248 characters for directory name
                    return false;
                }
                catch (ArgumentException)
                {
                    // Path.GetPathRoot does some fun things
                    // around legal combinations of characters that we miss 
                    // by simply checking against an array of legal characters
                    return true;
                }

                if (driveLetter == null)
                {
                    return false;
                }

                // lastly, check each directory name doesn't contain
                // any invalid filename characters
                var foldersInPath = str.Substring(driveLetter.Length);
                return foldersInPath.Split(new[] { '\\', '/' }, StringSplitOptions.None)
                             .Any(x => x.ContainsAny(Path.GetInvalidFileNameChars()));
            }, errorMessage);
        }

        public static ReactivePropertyValidator<string> IfPathNotRooted(this ReactivePropertyValidator<string> This, string errorMessage)
        {
            return This.IfFalse(Path.IsPathRooted, errorMessage);
        }

        public static ReactivePropertyValidator<string> DirectoryExists(this ReactivePropertyValidator<string> This, string errorMessage)
        {
            return This.IfFalse(Directory.Exists, errorMessage);
        }

        public static ReactivePropertyValidator<string> IfUncPath(this ReactivePropertyValidator<string> This, string errorMessage)
        {
            return This.IfTrue(str => str.StartsWith(@"\\", StringComparison.Ordinal), errorMessage);
        }
    }
}
