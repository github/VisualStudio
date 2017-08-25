using System;
using System.Reactive;
using Akavache;
using GitHub.Primitives;

namespace GitHub.Caches
{
    public interface IObservableKeychainAdapter : IDisposable
    {
        IObservable<LoginInfo> GetLoginAsync(HostAddress hostAddress);
        IObservable<Unit> SaveLogin(string user, string password, HostAddress hostAddress);
        IObservable<Unit> EraseLogin(HostAddress hostAddress);
        IObservable<Unit> Flush();
    }
}
