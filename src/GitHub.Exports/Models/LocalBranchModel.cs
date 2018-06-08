using GitHub.Services;
using GitHub.Primitives;
using LibGit2Sharp;

namespace GitHub.Models
{
    public class LocalBranchModel : BranchModel, IBranch
    {
        public LocalBranchModel(IRepository repository, Branch branch, IRepositoryModel repo, IGitService gitService)
            : base(branch?.FriendlyName, repo)
        {
            Sha = branch.Tip?.Sha;

            if (IsTracking = branch.IsTracking)
            {
                var trackedBranch = branch.TrackedBranch;
                TrackedSha = trackedBranch.Tip?.Sha;
                var trackedRemote = repository.Network.Remotes[trackedBranch.RemoteName];
                TrackedRemoteUrl = trackedRemote != null ? new UriString(trackedRemote.Url) : null;
                TrackedRemoteCanonicalName = trackedBranch.UpstreamBranchCanonicalName;
            }
        }
    }
}
