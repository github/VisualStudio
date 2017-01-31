using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.ViewModels;
using Octokit;
using ReactiveUI;
using NullGuard;
using System.Threading.Tasks;
using GitHub.Api;

namespace GitHub.Authentication
{
    [Export(typeof(ITwoFactorChallengeHandler))]
    [Export(typeof(IDelegatingTwoFactorChallengeHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TwoFactorChallengeHandler : ReactiveObject, IDelegatingTwoFactorChallengeHandler
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

        public async Task<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorAuthorizationException exception)
        {
            var userError = new TwoFactorRequiredUserError(exception);
            var result = await twoFactorDialog.Show(userError);

            if (result == RecoveryOptionResult.RetryOperation)
            {
                return userError.ChallengeResult;
            }
            else
            {
                throw exception;
            }
        }
    }
}