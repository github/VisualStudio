using System;
using GitHub.Primitives;
using GitHub.Collections;

namespace GitHub.Models
{
    public interface IBranch : ICopyable<IBranch>,
        IEquatable<IBranch>, IComparable<IBranch>
    {
        string Id { get; }
        string Name { get; }
        IRepositoryModel Repository { get; }
        string DisplayName { get; set; }
        string Sha { get; }
        bool IsTracking { get; }
        string TrackedSha { get; }
        UriString TrackedRemoteUrl { get; }
        string TrackedRemoteCanonicalName { get; }
    }
}
