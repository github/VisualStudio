using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media.Imaging;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public sealed class AccountDesigner : IAccount
    {
        public AccountDesigner()
        {
            Login = "octocat";
            IsUser = true;
        }

        public BitmapSource Avatar
        {
            get
            {
                return IsUser
                    ? AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png")
                    : AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");
            }
        }

        public bool HasMaximumPrivateRepositories { get; set; }
        public bool IsEnterprise { get; set; }
        public bool IsOnFreePlan { get; set; }
        public bool IsUser { get; set; }
        public string Login { get; set; }
        public int OwnedPrivateRepos { get; set; }
        public long PrivateReposInPlan { get; set; }

        public override string ToString()
        {
            return Login;
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

        public static bool operator >(AccountDesigner lhs, AccountDesigner rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return lhs?.CompareTo(rhs) > 0;
        }

        public static bool operator <(AccountDesigner lhs, AccountDesigner rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return (object)lhs == null || lhs.CompareTo(rhs) < 0;
        }

        public static bool operator ==(AccountDesigner lhs, AccountDesigner rhs)
        {
            return Equals(lhs, rhs) && ((object)lhs == null || lhs.CompareTo(rhs) == 0);
        }

        public static bool operator !=(AccountDesigner lhs, AccountDesigner rhs)
        {
            return !(lhs == rhs);
        }
        #endregion

    }
}