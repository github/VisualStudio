using GitHub.Models;
using System;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface ISynchronizeForkService
    {
        Task<bool> Sync(ILocalRepositoryModel repo);
    }
}
