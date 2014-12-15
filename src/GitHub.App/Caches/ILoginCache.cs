using System;
using System.Reactive;
using Akavache;

namespace GitHub
{
    public interface ILoginCache : IDisposable
    {
        IObservable<LoginInfo> GetLoginAsync(HostAddress hostAddress);
        IObservable<Unit> SaveLogin(string user, string password, HostAddress hostAddress);
        IObservable<Unit> EraseLogin(HostAddress hostAddress);
        IObservable<Unit> Flush();
    }
}
