using System;
using GitHub.Authentication;
using GitHub.Factories;
using GitHub.Primitives;
using ReactiveUI;

namespace GitHub.Models
{
    public interface IRepositoryHosts : IReactiveObject
    {
        IRepositoryHost EnterpriseHost { get; set; }
        IRepositoryHost GitHubHost { get; }
        IRepositoryHost LocalRepositoriesHost { get; }
        IObservable<AuthenticationResult> LogIn(
            HostAddress enterpriseHostAddress,
            string usernameOrEmail,
            string password);
        IRepositoryHostFactory RepositoryHostFactory { get; }
        bool IsLoggedInToAnyHost { get; }
        IRepositoryHost LookupHost(HostAddress address);
    }
}
