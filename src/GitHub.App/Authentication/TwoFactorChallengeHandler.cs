using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.ViewModels;
using Octokit;
using ReactiveUI;

namespace GitHub.Authentication
{
    [Export(typeof(ITwoFactorChallengeHandler))]
    public class TwoFactorChallengeHandler : ITwoFactorChallengeHandler
    {
        //readonly IServiceProvider serviceProvider;
        readonly Lazy<ITwoFactorViewModel> lazyTwoFactorDialog;

        [ImportingConstructor]
        public TwoFactorChallengeHandler(Lazy<ITwoFactorViewModel> twoFactorDialog)
        {
            //this.serviceProvider = serviceProvider;
            this.lazyTwoFactorDialog = twoFactorDialog;
        }

        public IObservable<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorRequiredException exception)
        {
            var twoFactorDialog = lazyTwoFactorDialog.Value as TwoFactorDialogViewModel;
            //var twoFactorView = (IViewFor<TwoFactorDialogViewModel>)serviceProvider.GetService(typeof(IViewFor<TwoFactorDialogViewModel>));

            return Observable.Start(() =>
            {
                //twoFactorView.ViewModel = twoFactorDialog;
                //((DialogWindow)twoFactorView).Show();

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