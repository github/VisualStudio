using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Exports;
using GitHub.Primitives;
using Microsoft.VisualStudio.Shell;
using Octokit;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    [ExportForProcess(typeof(ILoginManager), "devenv")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginManagerDispatcher : ILoginManager
    {
        readonly ILoginManager inner;

        [ImportingConstructor]
        public LoginManagerDispatcher([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            inner = serviceProvider.GetService(typeof(ILoginManager)) as ILoginManager;
        }

        public Task<User> Login(HostAddress hostAddress, IGitHubClient client, string userName, string password)
        {
            return inner.Login(hostAddress, client, userName, password);
        }

        public Task<User> LoginViaOAuth(
            HostAddress hostAddress,
            IGitHubClient client,
            IOauthClient oauthClient,
            Action<Uri> openBrowser,
            CancellationToken cancel)
        {
            return inner.LoginViaOAuth(hostAddress, client, oauthClient, openBrowser, cancel);
        }

        public Task<User> LoginFromCache(HostAddress hostAddress, IGitHubClient client)
        {
            return inner.LoginFromCache(hostAddress, client);
        }

        public Task Logout(HostAddress hostAddress, IGitHubClient client)
        {
            return inner.Logout(hostAddress, client);
        }

        public Task<User> LoginWithToken(HostAddress hostAddress, IGitHubClient client, string token)
        {
            return inner.LoginWithToken(hostAddress, client, token);
        }
    }
}
