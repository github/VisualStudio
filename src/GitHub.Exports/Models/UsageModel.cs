using System;

namespace GitHub.Models
{
    public class UsageModel
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Date { get; set; }
        public DimensionsModel Dimensions { get; set; }
        public MeasuresModel Measures { get; set; }

        public static UsageModel Create(Guid guid)
        {
            return new UsageModel
            {
                Guid = guid,
                Date = DateTime.Now,
                Dimensions = new DimensionsModel(),
                Measures = new MeasuresModel(),
            };
        }

        public class DimensionsModel
        {
            public string AppVersion { get; set; }
            public string VSVersion { get; set; }
            public string Lang { get; set; }
            public bool IsGitHubUser { get; set; }
            public bool IsEnterpriseUser { get; set; }
        }

        public class MeasuresModel
        {
            public int NumberOfStartups { get; set; }
            public int NumberOfUpstreamPullRequests { get; set; }
            public int NumberOfClones { get; set; }
            public int NumberOfReposCreated { get; set; }
            public int NumberOfReposPublished { get; set; }
            public int NumberOfGists { get; set; }
            public int NumberOfOpenInGitHub { get; set; }
            public int NumberOfLinkToGitHub { get; set; }
            public int NumberOfLogins { get; set; }
            public int NumberOfPullRequestsOpened { get; set; }
            public int NumberOfLocalPullRequestsCheckedOut { get; set; }
            public int NumberOfLocalPullRequestPulls { get; set; }
            public int NumberOfLocalPullRequestPushes { get; set; }
            public int NumberOfForkPullRequestsCheckedOut { get; set; }
            public int NumberOfForkPullRequestPulls { get; set; }
            public int NumberOfForkPullRequestPushes { get; set; }
            public int NumberOfWelcomeDocsClicks { get; set; }
            public int NumberOfWelcomeTrainingClicks { get; set; }
            public int NumberOfGitHubPaneHelpClicks { get; set; }
            public int NumberOfPRDetailsViewChanges { get; set; }
            public int NumberOfPRDetailsViewFile { get; set; }
            public int NumberOfPRDetailsCompareWithSolution { get; set; }
            public int NumberOfPRDetailsOpenFileInSolution { get; set; }
            public int NumberOfPRReviewDiffViewInlineCommentOpen { get; set; }
            public int NumberOfPRReviewDiffViewInlineCommentPost { get; set; }
        }
    }
}
