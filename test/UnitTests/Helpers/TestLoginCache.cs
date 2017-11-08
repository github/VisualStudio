using System;
using System.Reactive;
using Akavache;
using GitHub.Caches;
using GitHub.Primitives;

namespace UnitTests.Helpers
{
    public class TestLoginCache : ILoginCache
    {
        readonly LoginCache loginCacheDelegate = new LoginCache(new TestSharedCache());

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
