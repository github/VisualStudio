using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.ViewModels;
using Octokit;
using ReactiveUI;

namespace GitHub.Authentication
{
    [Export(typeof(ITwoFactorChallengeHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TwoFactorChallengeHandler : ITwoFactorChallengeHandler
    {
        ITwoFactorDialogViewModel twoFactorDialog;

        public void SetViewModel(ITwoFactorDialogViewModel vm)
        {
            twoFactorDialog = vm;
        }

        public IObservable<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorRequiredException exception)
        {
            return Observable.Start(() =>
            {
                var userError = new TwoFactorRequiredUserError(exception);
                return twoFactorDialog.Show(userError)
                    .SelectMany(x =>
                        x == RecoveryOptionResult.RetryOperation
                            ? Observable.Return(userError.ChallengeResult)
                            : Observable.Throw<TwoFactorChallengeResult>(exception));
            }, RxApp.MainThreadScheduler)
            .SelectMany(x => x)
            //.Finally(() =>
            //    ((DialogWindow)twoFactorView).Hide());
            ;
        }
    }
}