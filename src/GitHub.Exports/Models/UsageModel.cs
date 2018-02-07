using System;
using System.Reflection;

namespace GitHub.Models
{
    public struct UsageModel
    {
        public bool IsGitHubUser;
        public bool IsEnterpriseUser;
        public string AppVersion;
        public string VSVersion;
        public string Lang;
        public int NumberOfStartups;
        public int NumberOfStartupsWeek;
        public int NumberOfStartupsMonth;
        public int NumberOfUpstreamPullRequests;
        public int NumberOfClones;
        public int NumberOfReposCreated;
        public int NumberOfReposPublished;
        public int NumberOfGists;
        public int NumberOfOpenInGitHub;
        public int NumberOfLinkToGitHub;
        public int NumberOfLogins;
        public int NumberOfOAuthLogins;
        public int NumberOfTokenLogins;
        public int NumberOfPullRequestsOpened;
        public int NumberOfLocalPullRequestsCheckedOut;
        public int NumberOfLocalPullRequestPulls;
        public int NumberOfLocalPullRequestPushes;
        public int NumberOfForkPullRequestsCheckedOut;
        public int NumberOfForkPullRequestPulls;
        public int NumberOfForkPullRequestPushes;
        public int NumberOfWelcomeDocsClicks;
        public int NumberOfWelcomeTrainingClicks;
        public int NumberOfGitHubPaneHelpClicks;
        public int NumberOfPRDetailsViewChanges;
        public int NumberOfPRDetailsViewFile;
        public int NumberOfPRDetailsCompareWithSolution;
        public int NumberOfPRDetailsOpenFileInSolution;
        public int NumberOfPRDetailsNavigateToEditor;
        public int NumberOfPRReviewDiffViewInlineCommentOpen;
        public int NumberOfPRReviewDiffViewInlineCommentPost;

        public UsageModel Clone(bool includeWeekly, bool includeMonthly)
        {
            var result = this;
            if (!includeWeekly)
                result.NumberOfStartupsWeek = 0;
            if (!includeMonthly)
                result.NumberOfStartupsMonth = 0;
            return result;
        }

        public UsageModel ClearCounters(bool clearWeekly, bool clearMonthly)
        {
            var result = new UsageModel();
            if (!clearWeekly)
                result.NumberOfStartupsWeek = NumberOfStartupsWeek;
            if (!clearMonthly)
                result.NumberOfStartupsMonth = NumberOfStartupsMonth;

            result.IsGitHubUser = IsGitHubUser;
            result.IsEnterpriseUser = IsEnterpriseUser;
            result.AppVersion = AppVersion;
            result.VSVersion = VSVersion;
            result.Lang = Lang;
            return result;
        }
    }
}
