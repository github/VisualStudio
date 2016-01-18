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
        Uri baseUri;

        [ImportingConstructor]
        public LoginToGitHubViewModel(IRepositoryHosts repositoryHosts, IVisualStudioBrowser browser)
            : base(repositoryHosts, browser)
        {
            baseUri = HostAddress.GitHubDotComHostAddress.WebUri;

            NavigatePricing = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                browser.OpenUrl(GitHubUrls.Pricing);
                return Observable.Return(Unit.Default);

            });
        }

        public IReactiveCommand<Unit> NavigatePricing { get; private set; }

        protected override Uri BaseUri
            {
                get { return baseUri; }
            }

            protected override IObservable<AuthenticationResult> LogIn(object args)
            {
                return LogInToHost(HostAddress.GitHubDotComHostAddress);
            }
        }
}