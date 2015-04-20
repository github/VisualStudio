using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.ViewModels;
using Octokit;
using ReactiveUI;
using NullGuard;

namespace GitHub.Authentication
{
    [Export(typeof(ITwoFactorChallengeHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TwoFactorChallengeHandler : ReactiveObject, ITwoFactorChallengeHandler
    {
        ITwoFactorDialogViewModel twoFactorDialog;
        [AllowNull]
        public IViewModel CurrentViewModel
        {
            [return:AllowNull]
            get { return twoFactorDialog; }
            private set { this.RaiseAndSetIfChanged(ref twoFactorDialog, (ITwoFactorDialogViewModel)value); }
        }

        public void SetViewModel([AllowNull]IViewModel vm)
        {
            CurrentViewModel = vm;
        }

        public IObservable<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorAuthorizationException exception)
        {
            var userError = new TwoFactorRequiredUserError(exception);
            return twoFactorDialog.Show(userError)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(x =>
                    x == RecoveryOptionResult.RetryOperation
                        ? Observable.Return(userError.ChallengeResult)
                        : Observable.Throw<TwoFactorChallengeResult>(exception));
        }
    }
}