using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using NullGuard;
using ReactiveUI;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Account : ReactiveObject, IAccount
    {
        readonly ObservableAsPropertyHelper<BitmapSource> avatar;

        public Account(
            string login,
            bool isUser,
            bool isEnterprise,
            int ownedPrivateRepositoryCount,
            long privateRepositoryInPlanCount,
            IObservable<BitmapSource> bitmapSource)
        {
            Login = login;
            IsUser = isUser;
            IsEnterprise = isEnterprise;
            OwnedPrivateRepos = ownedPrivateRepositoryCount;
            PrivateReposInPlan = privateRepositoryInPlanCount;
            IsOnFreePlan = privateRepositoryInPlanCount == 0;
            HasMaximumPrivateRepositories = OwnedPrivateRepos >= PrivateReposInPlan;

            avatar = bitmapSource.ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, a => a.Avatar);
        }

        public bool IsOnFreePlan { get; private set; }

        public bool HasMaximumPrivateRepositories { get; private set; }

        public bool IsUser { get; private set; }

        public bool IsEnterprise { get; private set; }

        public string Login { get; private set; }

        public int OwnedPrivateRepos { get; private set; }

        public long PrivateReposInPlan { get; private set; }

        public BitmapSource Avatar
        {
            [return: AllowNull]
            get
            {
                return avatar.Value;
            }
        }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,
                    "Account: Login: {0} IsUser: {1}", Login, IsUser);
            }
        }
    }
}
