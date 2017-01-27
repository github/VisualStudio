using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Api
{
    public interface ISimpleApiClientFactory
    {
        Task<ISimpleApiClient> Create(UriString repositoryUrl);
        void ClearFromCache(ISimpleApiClient client);
    }
}
