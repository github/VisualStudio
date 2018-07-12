using GitHub.Exports;
using GitHub.Primitives;

namespace GitHub.Services
{
    /// <summary>
    /// Information used to map betwen a GitHub URL and some other context. This might be used to navigate
    /// between a GitHub URL and the location a repository file. Alternatively it might be used to map between
    /// the line a in a blame view and GitHub URL.
    /// </summary>
    public class GitHubContext
    {
        /// <summary>
        /// The owner of a repository.
        /// </summary>
        public string Owner { get; set; }
        /// <summary>
        /// The name of a repository.
        /// </summary>
        public string RepositoryName { get; set; }
        /// <summary>
        /// The host of a repository ("github.com" or a GitHub Enterprise host).
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// The name of a branch stored on GitHub (not the local branch name).
        /// </summary>
        public string BranchName { get; set; }
        /// <summary>
        /// Like a tree-ish but with ':' changed to '/' (e.g. "master/src" not "master:src").
        /// </summary>
        public string TreeishPath { get; set; }
        /// <summary>
        /// The name of a file on GitHub.
        /// </summary>
        public string BlobName { get; set; }
        /// <summary>
        /// A PR number if this context represents a PR.
        /// </summary>
        public int? PullRequest { get; set; }
        /// <summary>
        /// An issue number if this context represents an issue.
        /// </summary>
        public int? Issue { get; set; }
        /// <summary>
        /// The line number in a file.
        /// </summary>
        public int? Line { get; set; }
        /// <summary>
        /// The end line number if this context represents a range.
        /// </summary>
        public int? LineEnd { get; set; }
        /// <summary>
        /// The source Url of the context (when context originated from a URL).
        /// </summary>
        public UriString Url { get; set; }
        /// <summary>
        /// The type of context if known (blob, blame etc).
        /// </summary>
        public LinkType LinkType { get; set; }
    }
}
