using System;
using System.ComponentModel.Composition;
using GitHub.Authentication;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;

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
            return LogInToHost(HostAddress.GitHubDotComHostAddress);
        }
    }
}