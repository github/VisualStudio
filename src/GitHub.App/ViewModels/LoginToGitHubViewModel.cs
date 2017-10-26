using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Authentication;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [Export(typeof(ILoginToGitHubViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginToGitHubViewModel : LoginTabViewModel, ILoginToGitHubViewModel
    {
        [ImportingConstructor]
        public LoginToGitHubViewModel(IConnectionManager connectionManager, IVisualStudioBrowser browser)
            : base(connectionManager, browser)
        {
            BaseUri = HostAddress.GitHubDotComHostAddress.WebUri;

            NavigatePricing = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                browser.OpenUrl(GitHubUrls.Pricing);
                return Observable.Return(Unit.Default);

            });
        }

        public IReactiveCommand<Unit> NavigatePricing { get; }

        protected override Uri BaseUri { get; }

        protected override IObservable<AuthenticationResult> LogIn(object args)
        {
            return LogInToHost(HostAddress.GitHubDotComHostAddress);
        }
    }
}