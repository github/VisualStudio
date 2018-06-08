using GitHub.Primitives;

namespace GitHub.Models
{
    public interface ILocalBranch : IBranch
    {
        string Sha { get; }
        bool IsTracking { get; }
        string TrackedSha { get; }
        UriString TrackedRemoteUrl { get; }
        string TrackedRemoteCanonicalName { get; }
    }
}
