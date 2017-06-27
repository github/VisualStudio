using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using Microsoft.VisualStudio.Shell;
using Octokit;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    [Export(typeof(ILoginManager))]
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

        public Task<User> LoginFromCache(HostAddress hostAddress, IGitHubClient client)
        {
            return inner.LoginFromCache(hostAddress, client);
        }

        public Task Logout(HostAddress hostAddress, IGitHubClient client)
        {
            return inner.Logout(hostAddress, client);
        }
    }
}
