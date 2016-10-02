using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using GitHub.Caches;
using GitHub.Extensions.Reactive;
using GitHub.Primitives;
using Octokit;
using Serilog;

namespace GitHub.Services
{
    public class GitHubCredentialStore : ICredentialStore
    {
        readonly HostAddress hostAddress;
        readonly ILoginCache loginCache;

        public GitHubCredentialStore(HostAddress hostAddress, ILoginCache loginCache)
        {
            this.hostAddress = hostAddress;
            this.loginCache = loginCache;
        }

        public async Task<Credentials> GetCredentials()
        {
            return await loginCache.GetLoginAsync(hostAddress)
                .CatchNonCritical(Observable.Return(LoginCache.EmptyLoginInfo))
                .Select(CreateFromLoginInfo);
        }

        Credentials CreateFromLoginInfo(LoginInfo loginInfo)
        {
            if (loginInfo == LoginCache.EmptyLoginInfo)
            {
                Log.Debug("Could not retrieve login info from the secure cache '{CredentialCacheKeyHost}'", hostAddress.CredentialCacheKeyHost);
                return Credentials.Anonymous;
            }

            return new Credentials(loginInfo.UserName, loginInfo.Password);
        }
    }
}
