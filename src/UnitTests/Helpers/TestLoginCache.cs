using System;
using System.Reactive;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Primitives;
using NSubstitute;

namespace UnitTests.Helpers
{
    public class TestLoginCache : IObservableKeychainAdapter
    {
        static readonly IKeychain keychain = Substitute.For<IKeychain>();
        readonly ObservableKeychainAdapter loginCacheDelegate = new ObservableKeychainAdapter(keychain);

        public void Dispose()
        {
            loginCacheDelegate.Dispose();
        }

        public IObservable<LoginInfo> GetLoginAsync(HostAddress hostAddress)
        {
            return loginCacheDelegate.GetLoginAsync(hostAddress);
        }

        public IObservable<Unit> SaveLogin(string user, string password, HostAddress hostAddress)
        {
            return loginCacheDelegate.SaveLogin(user, password, hostAddress);
        }

        public IObservable<Unit> EraseLogin(HostAddress hostAddress)
        {
            return loginCacheDelegate.EraseLogin(hostAddress);
        }

        public IObservable<Unit> Flush()
        {
            return loginCacheDelegate.Flush();
        }
    }
}
