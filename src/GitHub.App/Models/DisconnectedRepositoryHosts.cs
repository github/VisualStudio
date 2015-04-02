
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Primitives;
using NullGuard;
using ReactiveUI;

namespace GitHub.Models
{
    public class DisconnectedRepositoryHost : ReactiveObject, IRepositoryHost
    {
        public DisconnectedRepositoryHost()
        {
            Address = HostAddress.Create(new Uri("https://null/"));
            Organizations = new ReactiveList<IAccount>();
            Accounts = new ReactiveList<IAccount>();
        }

        public HostAddress Address { get; private set; }
        public IApiClient ApiClient { get; private set; }

        [AllowNull]
        public IHostCache Cache
        {
            [return: AllowNull]
            get;
            private set;
        }
        public bool IsGitHub { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public bool IsLoggingIn { get; private set; }
        public bool IsEnterprise { get; private set; }
        public ReactiveList<IAccount> Organizations { get; private set; }
        public ReactiveList<IAccount> Accounts { get; private set; }
        public string Title { get; private set; }
        public IAccount User { get; private set; }
        public IObservable<IReadOnlyList<IAccount>> GetAccounts()
        {
            return Observable.Empty<IReadOnlyList<IAccount>>();
        }

        public IObservable<AuthenticationResult> LogIn(string usernameOrEmail, string password)
        {
            return Observable.Return(AuthenticationResult.CredentialFailure);
        }

        public IObservable<AuthenticationResult> LogInFromCache()
        {
            return Observable.Return(AuthenticationResult.CredentialFailure);
        }

        public IObservable<Unit> LogOut()
        {
            return Observable.Return(Unit.Default);
        }
    }
}
