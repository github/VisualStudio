using System;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Authentication;
using ReactiveUI;
using GitHub.Exports;

namespace GitHub.Models
{
    public class LocalRepositoriesHost : ReactiveObject, IRepositoryHost
    {
        readonly Lazy<IAccount> userThunk;

        public LocalRepositoriesHost()
        {
            userThunk = new Lazy<IAccount>(() => new LocalRepositoriesAccount(this));

            Address = null;
            ApiClient = null;
            Cache = null;
            IsGitHub = false;
            IsLoggedIn = true;
            IsLoggingIn = false;
            Organizations = new ReactiveList<IAccount>();
            Accounts = new ReactiveList<IAccount>();
            Title = "local";
        }

        public HostAddress Address { get; private set; }
        public IApiClient ApiClient { get; private set; }
        public IHostCache Cache { get; private set; }
        public bool IsGitHub { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public bool IsLoggingIn { get; private set; }
        public ReactiveList<IAccount> Organizations { get; private set; }
        public ReactiveList<IAccount> Accounts { get; private set; }
        public string Title { get; private set; }

        public IAccount User
        {
            get { return userThunk.Value; }
        }

        public IObservable<AuthenticationResult> LogIn(string usernameOrEmail, string password)
        {
            return Observable.Return(AuthenticationResult.Success);
        }

        public IObservable<AuthenticationResult> LogInFromCache()
        {
            return Observable.Return(AuthenticationResult.Success);
        }

        public IObservable<Unit> LogOut()
        {
            return Observable.Return(Unit.Default);
        }

        public IObservable<Unit> Refresh()
        {
            return Observable.Return(Unit.Default);
        }

        public IObservable<Unit> Refresh(Func<IRepositoryHost, IObservable<Unit>> refreshTrackedRepositoriesFunc)
        {
            return Observable.Return(Unit.Default);
        }

        bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { this.RaiseAndSetIfChanged(ref isSelected, value); }
        }

        public bool IsEnterprise
        {
            get { return false; }
        }

        public bool IsLocal
        {
            get { return true; }
        }
    }
}
