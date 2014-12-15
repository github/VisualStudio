using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using GitHub.Extensions.Reactive;
using NLog;
using Octokit;
using LogManager = NLog.LogManager;

namespace GitHub
{
    public class GitHubCredentialStore : ICredentialStore
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
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
                log.Debug(CultureInfo.InvariantCulture, "Could not retrieve login info from the secure cache '{0}'",
                    hostAddress.CredentialCacheKeyHost);
                return Credentials.Anonymous;
            }

            return new Credentials(loginInfo.UserName, loginInfo.Password);
        }
    }
}
