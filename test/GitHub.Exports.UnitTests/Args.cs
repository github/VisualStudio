using System;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using Octokit;

internal static class Args
{
    public static bool Boolean { get { return Arg.Any<bool>(); } }
    public static int Int32 { get { return Arg.Any<int>(); } }
    public static string String { get { return Arg.Any<string>(); } }
    public static Span Span { get { return Arg.Any<Span>(); } }
    public static SnapshotPoint SnapshotPoint { get { return Arg.Any<SnapshotPoint>(); } }
    public static NewRepository NewRepository { get { return Arg.Any<NewRepository>(); } }
    public static IAccount Account { get { return Arg.Any<IAccount>(); } }
    public static IServiceProvider ServiceProvider { get { return Arg.Any<IServiceProvider>(); } }
    public static HostAddress HostAddress { get { return Arg.Any<HostAddress>(); } }
    public static Uri Uri { get { return Arg.Any<Uri>(); } }
    public static LibGit2Sharp.IRepository LibGit2Repo { get { return Arg.Any<LibGit2Sharp.IRepository>(); } }
    public static LibGit2Sharp.Branch LibGit2Branch { get { return Arg.Any<LibGit2Sharp.Branch>(); } }
    public static Remote LibgGit2Remote { get { return Arg.Any<Remote>(); } }
    public static LocalRepositoryModel LocalRepositoryModel { get { return Arg.Any<LocalRepositoryModel>(); } }
    public static RemoteRepositoryModel RemoteRepositoryModel { get { return Arg.Any<RemoteRepositoryModel>(); } }
    public static BranchModel Branch { get { return Arg.Any<BranchModel>(); } }
    public static IGitService GitService { get { return Arg.Any<IGitService>(); } }
    public static Func<TwoFactorAuthorizationException, IObservable<TwoFactorChallengeResult>>
        TwoFactorChallengCallback
    { get { return Arg.Any<Func<TwoFactorAuthorizationException, IObservable<TwoFactorChallengeResult>>>(); } }
}
