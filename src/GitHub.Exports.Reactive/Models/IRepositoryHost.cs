using System;
using System.Reactive;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Primitives;
using ReactiveUI;

namespace GitHub.Models
{
    public interface IRepositoryHost
    {
        HostAddress Address { get; }
        IApiClient ApiClient { get; }
        IHostCache Cache { get; }
        bool IsGitHub { get; }
        bool IsLoggedIn { get; }
        bool IsLoggingIn { get; }
        bool IsEnterprise { get; }
        bool IsLocal { get; }
        ReactiveList<IAccount> Organizations { get; }
        ReactiveList<IAccount> Accounts { get; }
        string Title { get; }
        IAccount User { get; }

        IObservable<AuthenticationResult> LogIn(string usernameOrEmail, string password);
        IObservable<AuthenticationResult> LogInFromCache();
        IObservable<Unit> LogOut();
        IObservable<Unit> Refresh();
        IObservable<Unit> Refresh(Func<IRepositoryHost, IObservable<Unit>> refreshTrackedRepositoriesFunc);
    }
}
