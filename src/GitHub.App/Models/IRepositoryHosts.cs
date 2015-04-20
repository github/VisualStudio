using System;
using GitHub.Authentication;
using GitHub.Factories;
using GitHub.Primitives;
using ReactiveUI;

namespace GitHub.Models
{
    public interface IRepositoryHosts : IReactiveObject, IDisposable
    {
        IRepositoryHost EnterpriseHost { get; set; }
        IRepositoryHost GitHubHost { get; }
        IObservable<AuthenticationResult> LogIn(
            HostAddress hostAddress,
            string usernameOrEmail,
            string password);
        IObservable<AuthenticationResult> LogInFromCache(HostAddress hostAddress);
        IRepositoryHostFactory RepositoryHostFactory { get; }
        bool IsLoggedInToAnyHost { get; }
        IRepositoryHost LookupHost(HostAddress address);
    }
}
