using GitHub.Extensions;
using GitHub.Primitives;

namespace GitHub.Models
{
    public class GitReferenceModel
    {
        public GitReferenceModel(string @ref, string label, string sha, string repositoryCloneUri)
            : this(@ref, label, sha, new UriString(repositoryCloneUri))
        {
        }

        public GitReferenceModel(string @ref, string label, string sha, UriString repositoryCloneUri)
        {
            Guard.ArgumentNotEmptyString(@ref, nameof(@ref));
            Guard.ArgumentNotEmptyString(sha, nameof(sha));

            Ref = @ref;
            Label = label;
            Sha = sha;
            RepositoryCloneUrl = repositoryCloneUri;
        }

        public string Ref { get; }
        public string Label { get; }
        public string Sha { get; }
        public UriString RepositoryCloneUrl { get; }
    }
}
