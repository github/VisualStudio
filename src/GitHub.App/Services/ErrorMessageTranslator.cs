using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NullGuard;
using ReactiveUI;

namespace GitHub.Services
{
    public class ErrorMessageTranslator
    {
        readonly IDictionary<ErrorType, ErrorMap> userErrorMappings;

        public ErrorMessageTranslator(IDictionary<ErrorType, ErrorMap> userErrors)
        {
            userErrorMappings = userErrors;
        }

        public UserError GetUserError(ErrorType errorType, [AllowNull] Exception exception, params object[] context)
        {
            var translation = GetUserErrorTranslation(errorType, exception, context);
            return new UserError(translation.ErrorMessage, translation.CauseOrResolution, translation.RecoveryCommands, null, exception)
            {
                UserErrorIcon = StockUserErrorIcon.Error
            };
        }

        public UserErrorTranslation GetUserErrorTranslation(
            ErrorType errorType,
            [AllowNull] Exception exception,
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
                [AllowNull] string errorMessage,
                [AllowNull] string causeOrResolution,
                [AllowNull] IEnumerable<IRecoveryCommand> recoveryCommands)
            {
                ErrorMessage = errorMessage;
                CauseOrResolution = causeOrResolution;
                RecoveryCommands = recoveryCommands;
            }

            [AllowNull]
            public string ErrorMessage
            {
                [return: AllowNull]
                get;
                private set;
            }

            [AllowNull]
            public string CauseOrResolution
            {
                [return: AllowNull]
                get;
                private set;
            }

            [AllowNull]
            public IEnumerable<IRecoveryCommand> RecoveryCommands
            {
                [return: AllowNull]
                get;
                private set;
            }
        }
    }
}
