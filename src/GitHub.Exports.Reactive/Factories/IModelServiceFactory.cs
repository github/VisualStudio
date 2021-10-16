using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.Factories
{
    public interface IModelServiceFactory
    {
        Task<IModelService> CreateAsync(IConnection connection);
        IModelService CreateBlocking(IConnection connection);
    }
}
