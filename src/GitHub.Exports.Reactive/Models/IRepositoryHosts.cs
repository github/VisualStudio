using System;
using System.Threading.Tasks;
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
        Task EnsureInitialized();
        IObservable<AuthenticationResult> LogIn(
            HostAddress hostAddress,
            string username,
            string password);
        IRepositoryHostFactory RepositoryHostFactory { get; }
        bool IsLoggedInToAnyHost { get; }
        IRepositoryHost LookupHost(HostAddress address);
    }
}
