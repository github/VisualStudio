using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using GitHub.App;
using GitHub.Authentication;
using GitHub.Exports;
using GitHub.Info;
using GitHub.Services;
using GitHub.Validation;
using NullGuard;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.TwoFactor)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TwoFactorDialogViewModel : DialogViewModelBase, ITwoFactorDialogViewModel
    {
        bool isAuthenticationCodeSent;
        bool invalidAuthenticationCode;
        string authenticationCode;
        TwoFactorType twoFactorType;
        readonly ObservableAsPropertyHelper<string> description;
        readonly ObservableAsPropertyHelper<bool> isSms;
        readonly ObservableAsPropertyHelper<bool> showErrorMessage;

        [ImportingConstructor]
        public TwoFactorDialogViewModel(
            IVisualStudioBrowser browser,
            ITwoFactorChallengeHandler twoFactorChallengeHandler)
        {
            Title = Resources.TwoFactorTitle;
            twoFactorChallengeHandler.SetViewModel(this);

            var canVerify = this.WhenAny(
                x => x.AuthenticationCode,
                x => x.IsBusy,
                (code, busy) => !string.IsNullOrEmpty(code.Value) && code.Value.Length == 6 && !busy.Value);

            OkCommand = ReactiveCommand.Create(canVerify);
            NavigateLearnMore = ReactiveCommand.Create();
            NavigateLearnMore.Subscribe(x => browser.OpenUrl(GitHubUrls.TwoFactorLearnMore));
            //TODO: ShowHelpCommand.Subscribe(x => browser.OpenUrl(twoFactorHelpUri));
            ResendCodeCommand = ReactiveCommand.Create();

            showErrorMessage = this.WhenAny(
                x => x.IsAuthenticationCodeSent,
                x => x.InvalidAuthenticationCode,
                (authSent, invalid) => invalid.Value && !authSent.Value)
                .ToProperty(this, x => x.ShowErrorMessage);

            description = this.WhenAny(x => x.TwoFactorType, x => x.Value)
                .Select(type =>
                {
                    switch (type)
                    {
                        case TwoFactorType.Sms:
                            return Resources.TwoFactorSms;
                        case TwoFactorType.AuthenticatorApp:
                            return Resources.TwoFactorApp;
                        case TwoFactorType.Unknown:
                            return Resources.TwoFactorUnknown;

                        default:
                            return null;
                    }
                })
                .ToProperty(this, x => x.Description);

            isShowing = this.WhenAny(x => x.TwoFactorType, x => x.Value)
                .Select(factorType => factorType != TwoFactorType.None)
                .ToProperty(this, x => x.IsShowing);

            isSms = this.WhenAny(x => x.TwoFactorType, x => x.Value)
                .Select(factorType => factorType == TwoFactorType.Sms)
                .ToProperty(this, x => x.IsSms);
        }

        public IObservable<RecoveryOptionResult> Show(UserError userError)
        {
            IsBusy = false;
            var error = userError as TwoFactorRequiredUserError;
            Debug.Assert(error != null,
                String.Format(CultureInfo.InvariantCulture, "The user error is '{0}' not a TwoFactorRequiredUserError", userError));
            InvalidAuthenticationCode = error.RetryFailed;
            TwoFactorType = error.TwoFactorType;
            var ok = OkCommand
                .Do(_ => IsBusy = true)
                .Select(_ => AuthenticationCode == null
                    ? RecoveryOptionResult.CancelOperation
                    : RecoveryOptionResult.RetryOperation)
                .Do(_ => error.ChallengeResult = AuthenticationCode != null
                    ? new TwoFactorChallengeResult(AuthenticationCode)
                    : null);
            var resend = ResendCodeCommand.Select(_ => RecoveryOptionResult.RetryOperation)
                .Do(_ => error.ChallengeResult = TwoFactorChallengeResult.RequestResendCode);
            var cancel = CancelCommand.Select(_ => RecoveryOptionResult.CancelOperation);
            return Observable.Merge(ok, cancel, resend)
                .Take(1)
                .Do(_ => IsAuthenticationCodeSent = error.ChallengeResult == TwoFactorChallengeResult.RequestResendCode);
        }

        public TwoFactorType TwoFactorType
        {
            get { return twoFactorType; }
            private set { this.RaiseAndSetIfChanged(ref twoFactorType, value); }
        }

        public bool IsSms { get { return isSms.Value; } }

        public bool IsAuthenticationCodeSent
        {
            get { return isAuthenticationCodeSent; }
            private set { this.RaiseAndSetIfChanged(ref isAuthenticationCodeSent, value); }
        }

        public string Description
        {
            [return: AllowNull]
            get { return description.Value; }
        }

        [AllowNull]
        public string AuthenticationCode
        {
            [return: AllowNull]
            get { return authenticationCode; }
            set { this.RaiseAndSetIfChanged(ref authenticationCode, value); }
        }

        public ReactiveCommand<object> OkCommand { get; private set; }
        public ReactiveCommand<object> NavigateLearnMore { get; private set; }
        public ReactiveCommand<object> ResendCodeCommand { get; private set; }
        public ReactivePropertyValidator AuthenticationCodeValidator { get; private set; }

        public bool InvalidAuthenticationCode
        {
            get { return invalidAuthenticationCode; }
            private set { this.RaiseAndSetIfChanged(ref invalidAuthenticationCode, value); }
        }

        public bool ShowErrorMessage
        {
            get { return showErrorMessage.Value; }
        }
    }
}
