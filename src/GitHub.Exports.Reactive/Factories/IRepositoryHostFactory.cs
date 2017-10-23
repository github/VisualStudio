using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Factories
{
    public interface IRepositoryHostFactory
    {
        Task<IRepositoryHost> Create(IConnection connection);
    }
}