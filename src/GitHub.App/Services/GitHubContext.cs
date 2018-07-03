using System;

namespace GitHub.App.Services
{
    public class GitHubContext
    {
        public string Owner { get; set; }
        public string RepositoryName { get; set; }
        public string Host { get; set; }
        public string BranchName { get; set; }
        public string Treeish { get; set; }
        public string BlobName { get; set; }
        public int? PullRequest { get; set; }
        public int? Issue { get; set; }
        public int? Line { get; set; }
        public int? LineEnd { get; set; }
    }
}