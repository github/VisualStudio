using System;
using System.Collections.Generic;
using System.Linq;
using NullGuard;
using ReactiveUI;

namespace GitHub.Services
{
    public class ErrorMap
    {
        readonly ErrorMessage defaultMessage;
        readonly IEnumerable<Translation> translations;

        public ErrorMap(
            [AllowNull] ErrorMessage defaultMessage,
            [AllowNull] IEnumerable<Translation> translations,
            [AllowNull] IEnumerable<IRecoveryCommand> recoveryCommands)
        {
            this.defaultMessage = defaultMessage;
            this.translations = translations;
            RecoveryCommands = recoveryCommands;
        }

        [AllowNull]
        public IEnumerable<IRecoveryCommand> RecoveryCommands
        {
            [return: AllowNull]
            get;
            private set;
        }

        [return: AllowNull]
        public ErrorMessage GetErrorInfo([AllowNull] Exception exception)
        {
            if (exception != null && translations != null)
            {
                var translated = (from t in translations
                    let result = t.Translate(exception)
                    where result != null
                    select result).FirstOrDefault();

                if (translated != null)
                    return translated;
            }
            return defaultMessage;
        }
    }
}
