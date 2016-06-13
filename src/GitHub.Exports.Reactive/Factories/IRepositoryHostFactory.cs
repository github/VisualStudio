using System;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Factories
{
    public interface IRepositoryHostFactory : IDisposable
    {
        IRepositoryHost Create(HostAddress hostAddress);
        void Remove(IRepositoryHost host);
    }
}