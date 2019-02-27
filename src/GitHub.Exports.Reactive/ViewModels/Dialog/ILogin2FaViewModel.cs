using System;
using System.Reactive;
using GitHub.Validation;
using Octokit;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace GitHub.ViewModels.Dialog
{
    public interface ILogin2FaViewModel : IDialogContentViewModel
    {
        ReactiveCommand<Unit, Unit> OkCommand { get; }
        ReactiveCommand<Unit, Unit> NavigateLearnMore { get; }
        ReactiveCommand<Unit, Unit> ResendCodeCommand { get; }

#pragma warning disable CS0618 // Type or member is obsolete
        IObservable<TwoFactorChallengeResult> Show(UserError error);
#pragma warning restore CS0618 // Type or member is obsolete
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
