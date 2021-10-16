using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace GitHub.Services
{
    public class ErrorMap
    {
        readonly ErrorMessage defaultMessage;
        readonly IEnumerable<Translation> translations;

        public ErrorMap(
            ErrorMessage defaultMessage,
            IEnumerable<Translation> translations,
            IEnumerable<IRecoveryCommand> recoveryCommands)
        {
            this.defaultMessage = defaultMessage;
            this.translations = translations;
            RecoveryCommands = recoveryCommands;
        }

        public IEnumerable<IRecoveryCommand> RecoveryCommands
        {
            get;
            private set;
        }

        public ErrorMessage GetErrorInfo(Exception exception)
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
