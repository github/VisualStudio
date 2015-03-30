using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Authentication;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [Export(typeof(ILoginToGitHubViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginToGitHubViewModel : LoginTabViewModel, ILoginToGitHubViewModel
    {
        readonly Lazy<Uri> baseUri;

        [ImportingConstructor]
        public LoginToGitHubViewModel(IRepositoryHosts repositoryHosts, IVisualStudioBrowser browser)
            : base(repositoryHosts, browser)
        {
            baseUri = new Lazy<Uri>(() => repositoryHosts.GitHubHost.Address.WebUri);
        }

        protected override Uri BaseUri
        {
            get
            {
                return baseUri.Value;
            }
        }

        protected override IObservable<AuthenticationResult> LogIn(object args)
        {
            ShowLogInFailedError = false;
            ShowTwoFactorAuthFailedError = false;

            return RepositoryHosts.LogInGitHubHost(UsernameOrEmail, Password)
                .SelectMany(authResult =>
                {
                    if (authResult == AuthenticationResult.CredentialFailure)
                    {
                        ShowLogInFailedError = true;

                        Password = "";
                        return Observable.FromAsync(PasswordValidator.ResetAsync)
                            .Select(_ => authResult);
                    }
                    else if (authResult == AuthenticationResult.VerificationFailure)
                    {
                        ShowTwoFactorAuthFailedError = true;
                    }
                    else if (authResult == AuthenticationResult.Success)
                    {
                        return Observable.Return(authResult);
                        // Reset.ExecuteAsync()
                    }

                    return Observable.Return(authResult);
                });
        }
    }
}