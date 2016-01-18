using System;

namespace GitHub.Api
{
    public interface ISimpleApiClientFactory
    {
        ISimpleApiClient Create(Uri repoUrl);
        void ClearFromCache(ISimpleApiClient client);
    }
}
