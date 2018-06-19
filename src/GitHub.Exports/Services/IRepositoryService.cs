using System;
using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Services
{
    public interface IRepositoryService
    {
        Task<string> ReadParentOwnerLogin(
            HostAddress address,
            string owner,
            string name);
    }
}
