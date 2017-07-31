using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GitHub.Extensions;
using Newtonsoft.Json;
using Octokit;

namespace GitHub.Services
{
    public class Translation
    {
        readonly ErrorMessage defaultMessage;
        readonly Func<Exception, ErrorMessage> translator;

        public Translation(string original, string heading, string message)
        {
            Guard.ArgumentNotNull(heading, nameof(heading));
            Guard.ArgumentNotNull(message, nameof(message));

            Original = original;
            defaultMessage = new ErrorMessage(heading, message);
        }

        public Translation(string original, Func<Exception, ErrorMessage> translator)
        {
            Guard.ArgumentNotNull(translator, nameof(translator));

            Original = original;
            this.translator = translator;
        }

        public Translation(string original, string heading, string messageFormatString, Func<Exception, string> translator)
            : this(original, heading, messageFormatString)
        {
            Guard.ArgumentNotNull(heading, nameof(heading));
            Guard.ArgumentNotNull(messageFormatString, nameof(messageFormatString));
            Guard.ArgumentNotNull(translator, nameof(translator));

            this.translator = e => new ErrorMessage(heading, String.Format(CultureInfo.InvariantCulture, messageFormatString, translator(e)));
        }

        public string Original { get; private set; }

        public ErrorMessage Translate(Exception exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            var match = Match(exception);
            if (match == null) return null;

            if (translator == null)
            {
                var exceptionMessageLine = match.Item2;
                if (exceptionMessageLine != null)
                {
                    var heading = defaultMessage.HeadingHasPlaceholders
                        ? Regex.Replace(exceptionMessageLine, Original, defaultMessage.Heading)
                        : defaultMessage.Heading;

                    var message = defaultMessage.MessageHasPlaceholders
                        ? Regex.Replace(exceptionMessageLine, Original, defaultMessage.Message)
                        : defaultMessage.Message;
                    return new ErrorMessage(heading, message);
                }

                return defaultMessage;
            }

            return translator(exception);
        }

        // Returns a tuple indicating whether this translation is a match for the exception and the regex line if existing.
        protected virtual Tuple<Translation, string> Match(Exception exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            string exceptionMessage = exception.Message;

            var apiException = exception as ApiValidationException;
            var error = apiException?.ApiError?.Errors?.FirstOrDefault();
            if (error != null)
                exceptionMessage = error.Message ?? exceptionMessage;

            if (Original == exceptionMessage) return new Tuple<Translation, string>(this, null);

            var exceptionMessageLines = exceptionMessage.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (exceptionMessageLines.Any(l => l == Original)) return new Tuple<Translation, string>(this, null);

            try
            {
                // If the response is a JSON response from the API, let's 
                // look at all the messages for an exact match.
                var githubError = 
                    exceptionMessage.StartsWith('{') || exceptionMessage.StartsWith('[')
                    ? JsonConvert.DeserializeObject<ApiError>(exceptionMessage) 
                    : null;
                if (githubError != null 
                    && (githubError.Message == Original 
                    || (githubError.Errors != null && githubError.Errors.Any(e => e.Message == Original))))
                {
                    return new Tuple<Translation, string>(this, null);
                }
            }
            catch (Exception)
            {
                // Ignore. We were probably wrong about this being a proper GitHubError API response.
            }

            var regexMatchingLine = exceptionMessageLines.FirstOrDefault(l => Regex.IsMatch(l, Original, RegexOptions.IgnoreCase));
            return regexMatchingLine != null ? new Tuple<Translation, string>(this, regexMatchingLine) : null;
        }
    }

    public class Translation<TException> : Translation where TException : Exception
    {
        public Translation(string heading, string message) : base(null, heading, message)
        {
            Guard.ArgumentNotNull(heading, nameof(heading));
            Guard.ArgumentNotNull(message, nameof(message));
        }

        public Translation(string heading) : base(null, e => new ErrorMessage(heading, e.Message))
        {
            Guard.ArgumentNotNull(heading, nameof(heading));
        }

        protected override Tuple<Translation, string> Match(Exception exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            if (exception is TException) return new Tuple<Translation, string>(null, null);
            return null;
        }
    }
}
