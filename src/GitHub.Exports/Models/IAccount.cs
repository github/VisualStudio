using System;
using System.Windows.Media.Imaging;
using GitHub.Collections;

namespace GitHub.Models
{
    public interface IAccount : ICopyable<IAccount>,
        IEquatable<IAccount>, IComparable<IAccount>
    {
        bool IsEnterprise { get; }
        bool IsOnFreePlan { get; }
        bool HasMaximumPrivateRepositories { get; }
        bool IsUser { get; }
        string Login { get; }
        int OwnedPrivateRepos { get; }
        long PrivateReposInPlan { get; }
        string AvatarUrl { get; }
        BitmapSource Avatar { get; }
    }
}
