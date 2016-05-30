using System;

namespace GitHub.Models
{
    public class UsageModel
    {
        public bool IsGitHubUser { get; set; }
        public bool IsEnterpriseUser { get; set; }
        public bool InstalledCommandLineTools { get; set; }
        public int NumberOfRepositories { get; set; }
        public int NumberOfGitHubRepositories { get; set; }
        public int NumberOfGitHubForks { get; set; }
        public int NumberOfOrgs { get; set; }
        public int NumberOfCommits { get; set; }
        public int NumberOfClones { get; set; }
        public int NumberOfSyncs { get; set; }
        public string AppVersion { get; set; }
        public string Lang { get; set; }
        // I know you want to rename this to OSVersion. Resist the urge, this is convention based
        // pascal to camel case.
        public string OsVersion { get; set; }
        public bool Is64BitOperatingSystem { get; set; }
        public int RamMB { get; set; }
        public int NumberOfStartups { get; set; }
        public int NumberOfStartupsWeek { get; set; }
        public int NumberOfStartupsMonth { get; set; }
        public int NumberOfPartialCommits { get; set; }
        public int NumberOfTutorialRuns { get; set; }
        public int NumberOfOpenOnDisks { get; set; }
        public int NumberOfOpenInShells { get; set; }
        public int SecondsSinceLaunch { get; set; }
        public int NumberOfRepositoryOwners { get; set; }
        public int NumberOfBranchSwitches { get; set; }
        public int NumberOfDiscardChanges { get; set; }
        public int NumberOfOpenedURLs { get; set; }
        public int NumberOfLFSDiffs { get; set; }
        public int NumberOfMergeCommits { get; set; }
        public int NumberOfMergeConflicts { get; set; }
        public int NumberOfOpenInExternalEditors { get; set; }
        public int NumberOfUpstreamPullRequests { get; set; }
    }
}
