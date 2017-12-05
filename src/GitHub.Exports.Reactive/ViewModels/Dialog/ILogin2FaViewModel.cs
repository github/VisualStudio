using System;
using GitHub.Validation;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    public interface ILogin2FaViewModel : IDialogContentViewModel
    {
        ReactiveCommand<object> OkCommand { get; }
        ReactiveCommand<object> NavigateLearnMore { get; }
        ReactiveCommand<object> ResendCodeCommand { get; }

        IObservable<TwoFactorChallengeResult> Show(UserError error);
        void Cancel();

        bool IsBusy { get; }
        bool IsSms { get; }
        bool IsAuthenticationCodeSent { get; }
        bool ShowErrorMessage { get; }
        bool InvalidAuthenticationCode { get; }
        string Description { get; }
        string AuthenticationCode { get; set; }
        TwoFactorType TwoFactorType { get; }

        /// <summary>
        /// Gets the validator instance used for validating the
        /// <see cref="AuthenticationCode"/> property
        /// </summary>
        ReactivePropertyValidator AuthenticationCodeValidator { get; }
    }
}
