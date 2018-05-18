using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.Extensions;
using GitHub.Primitives;
using Octokit;
using ReactiveUI;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Account : ReactiveObject, IAccount
    {
        BitmapSource avatar;
        IObservable<BitmapSource> bitmapSource;
        IDisposable bitmapSourceSubscription;

        public Account(
            string login,
            bool isUser,
            bool isEnterprise,
            int ownedPrivateRepositoryCount,
            long privateRepositoryInPlanCount,
            string avatarUrl,
            IObservable<BitmapSource> bitmapSource)
        {
            Guard.ArgumentNotEmptyString(login, nameof(login));

            Login = login;
            IsUser = isUser;
            IsEnterprise = isEnterprise;
            OwnedPrivateRepos = ownedPrivateRepositoryCount;
            PrivateReposInPlan = privateRepositoryInPlanCount;
            IsOnFreePlan = privateRepositoryInPlanCount == 0;
            HasMaximumPrivateRepositories = OwnedPrivateRepos >= PrivateReposInPlan;
            AvatarUrl = avatarUrl;
            this.bitmapSource = bitmapSource;

            bitmapSourceSubscription = bitmapSource
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Avatar = x);
        }

        public Account(Octokit.Account account)
        {
            Guard.ArgumentNotNull(account, nameof(account));

            Login = account.Login;
            IsUser = (account as User) != null;
            Uri htmlUrl;
            IsEnterprise = Uri.TryCreate(account.HtmlUrl, UriKind.Absolute, out htmlUrl)
                && !HostAddress.IsGitHubDotComUri(htmlUrl);
            PrivateReposInPlan = account.Plan != null ? account.Plan.PrivateRepos : 0;
            OwnedPrivateRepos = account.OwnedPrivateRepos;
            IsOnFreePlan = PrivateReposInPlan == 0;
            HasMaximumPrivateRepositories = OwnedPrivateRepos >= PrivateReposInPlan;
            AvatarUrl = account.AvatarUrl;
        }

        public Account(Octokit.Account account, IObservable<BitmapSource> bitmapSource)
            : this(account)
        {
            bitmapSource.ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Avatar = x);
        }

        public bool IsOnFreePlan { get; private set; }

        public bool HasMaximumPrivateRepositories { get; private set; }

        public bool IsUser { get; private set; }

        public bool IsEnterprise { get; private set; }

        public string Login { get; private set; }

        public int OwnedPrivateRepos { get; private set; }

        public long PrivateReposInPlan { get; private set; }

        public string AvatarUrl { get; set; }

        public BitmapSource Avatar
        {
            get { return avatar; }
            set { avatar = value; this.RaisePropertyChanged(); }
        }

        #region Equality things
        public void CopyFrom(IAccount other)
        {
            if (!Equals(other))
                throw new ArgumentException("Instance to copy from doesn't match this instance. this:(" + this + ") other:(" + other + ")", nameof(other));
            OwnedPrivateRepos = other.OwnedPrivateRepos;
            PrivateReposInPlan = other.PrivateReposInPlan;
            IsOnFreePlan = other.IsOnFreePlan;
            HasMaximumPrivateRepositories = other.HasMaximumPrivateRepositories;
            Avatar = other.Avatar;

            var otherAccount = other as Account;
            if (otherAccount != null)
            {
                bitmapSourceSubscription.Dispose();

                bitmapSourceSubscription = otherAccount.bitmapSource
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => Avatar = x);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as Account;
            return other != null && Login == other.Login && IsUser == other.IsUser && IsEnterprise == other.IsEnterprise;
        }

        public override int GetHashCode()
        {
            return (Login?.GetHashCode() ?? 0) ^ IsUser.GetHashCode() ^ IsEnterprise.GetHashCode();
        }

        bool IEquatable<IAccount>.Equals(IAccount other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Login == other.Login && IsUser == other.IsUser && IsEnterprise == other.IsEnterprise;
        }

        public int CompareTo(IAccount other)
        {
            return other != null ? String.Compare(Login, other.Login, StringComparison.CurrentCulture) : 1;
        }

        public static bool operator >(Account lhs, Account rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return lhs?.CompareTo(rhs) > 0;
        }

        public static bool operator <(Account lhs, Account rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return (object)lhs == null || lhs.CompareTo(rhs) < 0;
        }

        public static bool operator ==(Account lhs, Account rhs)
        {
            return Equals(lhs, rhs) && ((object)lhs == null || lhs.CompareTo(rhs) == 0);
        }

        public static bool operator !=(Account lhs, Account rhs)
        {
            return !(lhs == rhs);
        }
        #endregion

        public override string ToString()
        {
            return Login;
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
