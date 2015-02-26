using System;
using System.Text.RegularExpressions;

namespace GitHub.Services
{
    public class ErrorMessage
    {
        static readonly Regex placeholderRegex = new Regex(@"\$\d", RegexOptions.Compiled);
        readonly Lazy<bool> headingHasPlaceholders;
        readonly Lazy<bool> messageHasPlaceholders;

        public ErrorMessage(string heading, string message)
        {
            Heading = heading;
            headingHasPlaceholders = new Lazy<bool>(() => placeholderRegex.IsMatch(heading));
            Message = message;
            messageHasPlaceholders = new Lazy<bool>(() => placeholderRegex.IsMatch(message));
        }

        public string Heading { get; private set; }
        public string Message { get; private set; }

        public bool HeadingHasPlaceholders
        {
            get { return headingHasPlaceholders.Value; }
        }

        public bool MessageHasPlaceholders
        {
            get { return messageHasPlaceholders.Value; }
        }
    }
}
