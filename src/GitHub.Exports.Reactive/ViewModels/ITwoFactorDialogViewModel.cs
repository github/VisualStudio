using System;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface ITwoFactorDialogViewModel : IDialogViewModel
    {
        ReactiveCommand<object> OkCommand { get; }
        ReactiveCommand<object> NavigateLearnMore { get; }
        ReactiveCommand<object> ResendCodeCommand { get; }

        IObservable<RecoveryOptionResult> Show(UserError error);

        bool IsSms { get; }
        bool IsAuthenticationCodeSent { get; }
        bool ShowErrorMessage { get; }
        bool InvalidAuthenticationCode { get; }
        string Description { get; }
        string AuthenticationCode { get; set; }

        /// <summary>
        /// Gets the validator instance used for validating the
        /// <see cref="AuthenticationCode"/> property
        /// </summary>
        ReactivePropertyValidator AuthenticationCodeValidator { get; }
    }
}
