using GitHub.Services;
using GitHub.Primitives;
using LibGit2Sharp;

namespace GitHub.Models
{
    public class LocalBranchModel : BranchModel, IBranch
    {
        public LocalBranchModel(Branch branch, IRepositoryModel repo, IGitService gitService)
            : base(branch?.FriendlyName, repo)
        {
            Sha = branch.Tip?.Sha;

            if(IsTracking = branch.IsTracking)
            {
                var trackedBranch = branch.TrackedBranch;
                TrackedSha = trackedBranch.Tip?.Sha;
#pragma warning disable 0618 // TODO: Replace `Branch.Remote` with `Repository.Network.Remotes[branch.RemoteName]`.
                TrackedRemoteUrl = new UriString(trackedBranch.Remote.Url);
#pragma warning restore 0618
                TrackedRemoteCanonicalName = trackedBranch.UpstreamBranchCanonicalName;
            }
        }
    }
}
