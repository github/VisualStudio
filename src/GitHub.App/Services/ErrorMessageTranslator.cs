using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GitHub.Extensions;
using ReactiveUI;
using ReactiveUI.Legacy;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CA1034 // Nested types should not be visible

namespace GitHub.Services
{
    public class ErrorMessageTranslator
    {
        readonly IDictionary<ErrorType, ErrorMap> userErrorMappings;

        public ErrorMessageTranslator(IDictionary<ErrorType, ErrorMap> userErrors)
        {
            Guard.ArgumentNotNull(userErrors, nameof(userErrors));

            userErrorMappings = userErrors;
        }

        public UserError GetUserError(ErrorType errorType, Exception exception, params object[] context)
        {
            var translation = GetUserErrorTranslation(errorType, exception, context);
            return new UserError(translation.ErrorMessage, translation.CauseOrResolution, translation.RecoveryCommands, null, exception)
            {
                UserErrorIcon = StockUserErrorIcon.Error
            };
        }

        public UserErrorTranslation GetUserErrorTranslation(
            ErrorType errorType,
            Exception exception,
            params object[] context)
        {
            ErrorMessage errorMessage = null;
            ErrorMap errorMap;
            if (userErrorMappings.TryGetValue(ErrorType.Global, out errorMap) && errorMap != null)
            {
                errorMessage = errorMap.GetErrorInfo(exception);
            }

            if (errorMessage == null)
            {
                if (!userErrorMappings.TryGetValue(errorType, out errorMap) || errorMap == null)
                    throw new InvalidOperationException("This should never happen!");
            }

            errorMessage = errorMap.GetErrorInfo(exception) ?? new ErrorMessage("error", "Unknown error occurred");
            string details = errorMessage.Message;
            string heading = errorMessage.Heading;
            if (context != null && context.Any())
            {
                heading = String.Format(CultureInfo.InvariantCulture, heading, context);
                details = String.Format(CultureInfo.InvariantCulture, details, context);
            }

            return new UserErrorTranslation(heading, details, errorMap.RecoveryCommands);
        }

        public class UserErrorTranslation
        {
            public UserErrorTranslation(
                string errorMessage,
                string causeOrResolution,
                IEnumerable<IRecoveryCommand> recoveryCommands)
            {
                ErrorMessage = errorMessage;
                CauseOrResolution = causeOrResolution;
                RecoveryCommands = recoveryCommands;
            }

            public string ErrorMessage
            {
                get;
                private set;
            }

            public string CauseOrResolution
            {
                get;
                private set;
            }

            public IEnumerable<IRecoveryCommand> RecoveryCommands
            {
                get;
                private set;
            }
        }
    }
}
