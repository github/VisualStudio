using System.ComponentModel.Composition;
using GitHub.Caches;
using GitHub.Models;
using Account = GitHub.Models.Account;

namespace GitHub.Factories
{
    [Export(typeof(IAccountFactory))]
    public class AccountFactory : IAccountFactory
    {
        public IAccount CreateAccount(CachedAccount account)
        {
            return new Account(account);
        }
    }
}
