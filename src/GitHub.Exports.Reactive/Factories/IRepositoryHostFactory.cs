using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Factories
{
    public interface IRepositoryHostFactory : IDisposable
    {
        Task<IRepositoryHost> Create(HostAddress hostAddress);
        void Remove(IRepositoryHost host);
    }
}