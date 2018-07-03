using System;

namespace GitHub.App.Services
{
    public class GitHubContext
    {
        public string Owner { get; set; }
        public string RepositoryName { get; set; }
        public string Host { get; set; }
        public string Branch { get; set; }
        public int? PullRequest { get; set; }
        public int? Issue { get; set; }
        public string Path { get; set; }
        public int? Line { get; set; }
        public int? LineEnd { get; set; }
    }
}