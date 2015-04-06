using System;
using GitHub.Authentication;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface ITwoFactorDialogViewModel : IViewModel
    {
        ReactiveCommand<object> OkCommand { get; }
        ReactiveCommand<object> CancelCommand { get; }
        ReactiveCommand<RecoveryOptionResult> ShowHelpCommand { get; }
        ReactiveCommand<RecoveryOptionResult> ResendCodeCommand { get; }

        IObservable<RecoveryOptionResult> Show(UserError error);

        bool IsSms { get; }
        bool IsAuthenticationCodeSent { get; }
        string Description { get; }
        string AuthenticationCode { get; set; }

        /// <summary>
        /// Gets the validator instance used for validating the
        /// <see cref="AuthenticationCode"/> property
        /// </summary>
        ReactivePropertyValidator AuthenticationCodeValidator { get; }
    }
}
