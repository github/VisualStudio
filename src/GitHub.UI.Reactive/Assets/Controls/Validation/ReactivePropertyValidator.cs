using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHub.Extensions;
using ReactiveUI;

namespace GitHub.UI
{
    public class ReactivePropertyValidationResult
    {
        public bool IsValid { get; private set; }
        public ValidationStatus Status { get; private set; }
        public string Message { get; private set; }

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It is immutable")]
        public static readonly ReactivePropertyValidationResult Success = new ReactivePropertyValidationResult(ValidationStatus.Valid);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It is immutable")]
        public static readonly ReactivePropertyValidationResult Unvalidated = new ReactivePropertyValidationResult();

        public ReactivePropertyValidationResult()
            : this(ValidationStatus.Unvalidated, "")
        {
        }

        public ReactivePropertyValidationResult(ValidationStatus validationStatus)
            : this(validationStatus, "")
        {
        }

        public ReactivePropertyValidationResult(ValidationStatus validationStatus, string message)
        {
            Status = validationStatus;
            IsValid = validationStatus != ValidationStatus.Invalid;
            Message = message;
        }
    }

    public enum ValidationStatus
    {
        Unvalidated = 0,
        Invalid = 1,
        Valid = 2,
    }

    public abstract class ReactivePropertyValidator : ReactiveObject
    {
        public static ReactivePropertyValidator<TProp> For<TObj, TProp>(TObj This, Expression<Func<TObj, TProp>> property)
        {
            return new ReactivePropertyValidator<TObj, TProp>(This, property);
        }

        public abstract ReactivePropertyValidationResult ValidationResult { get; protected set; }

        public abstract bool IsValidating { get; }

        protected ReactivePropertyValidator()
        {
        }

        public abstract Task ExecuteAsync();

        public abstract Task ResetAsync();
    }

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ReactivePropertyValidator<TProp> : ReactivePropertyValidator
    {
        readonly ReactiveCommand<ReactivePropertyValidationResult> validateCommand;
        ReactivePropertyValidationResult validationResult;

        public override ReactivePropertyValidationResult ValidationResult
        {
            get { return validationResult; }
            protected set { this.RaiseAndSetIfChanged(ref validationResult, value); }
        }

        public override Task ExecuteAsync()
        {
            return validateCommand.ExecuteAsyncTask(new ValidationParameter());
        }

        public override Task ResetAsync()
        {
            return validateCommand.ExecuteAsyncTask(new ValidationParameter { RequiresReset = true });
        }

        readonly List<Func<TProp, IObservable<ReactivePropertyValidationResult>>> validators =
            new List<Func<TProp, IObservable<ReactivePropertyValidationResult>>>();

        readonly ObservableAsPropertyHelper<bool> isValidating;
        public override bool IsValidating
        {
            get { return isValidating.Value; }
        }

        public ReactivePropertyValidator(IObservable<TProp> signal)
        {
            validateCommand = ReactiveCommand.CreateAsyncObservable(param =>
            {
                var validationParams = (ValidationParameter)param;

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

            isValidating = validateCommand.IsExecuting.ToProperty(this, x => x.IsValidating);

            validateCommand.Subscribe(x => ValidationResult = x);
            signal.Subscribe(x => validateCommand.Execute(new ValidationParameter { PropertyValue = x, RequiresReset = false }));
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
        protected ReactivePropertyValidator()
            : base(Observable.Empty<TProp>())
        {
        }

        public ReactivePropertyValidator(TObj This, Expression<Func<TObj, TProp>> property)
            : base(This.WhenAny(property, x => x.Value)) { }
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

        public static ReactivePropertyValidator<string> IfNotUri(this ReactivePropertyValidator<string> This, string errorMessage)
        {
            return This.IfFalse(s =>
            {
                Uri uri;
                return Uri.TryCreate(s, UriKind.Absolute, out uri);
            }, errorMessage);
        }

        public static ReactivePropertyValidator<string> IfSameAsHost(this ReactivePropertyValidator<string> This, Uri compareToHost, string errorMessage)
        {
            return This.IfTrue(s =>
            {
                Uri uri;
                var isUri = Uri.TryCreate(s, UriKind.Absolute, out uri);
                return isUri && uri.IsSameHost(compareToHost);

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

        public static ReactivePropertyValidator<string> IfUncPath(this ReactivePropertyValidator<string> This, string errorMessage)
        {
            return This.IfTrue(str => str.StartsWith(@"\\", StringComparison.Ordinal), errorMessage);
        }
    }
}