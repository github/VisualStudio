using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Authentication;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Represents the Login dialog content.
    /// </summary>
    [Export(typeof(ILoginViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginViewModel : PagedDialogViewModelBase, ILoginViewModel
    {
        [ImportingConstructor]
        public LoginViewModel(
            ILoginCredentialsViewModel credentials,
            ILogin2FaViewModel twoFactor,
            IDelegatingTwoFactorChallengeHandler twoFactorHandler)
        {
            twoFactorHandler.SetViewModel(twoFactor);

            Content = credentials;
            Done = credentials.Done;            

            twoFactor.WhenAnyValue(x => x.TwoFactorType)
                .Subscribe(x =>
                {
                    Content = x == TwoFactorType.None ?
                        (IDialogContentViewModel)credentials :
                        twoFactor;
                });
        }

        /// <inheritdoc/>
        public override IObservable<object> Done { get; }
    }
}
