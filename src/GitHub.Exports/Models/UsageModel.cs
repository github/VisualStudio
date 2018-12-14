using System;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1720 // Identifier contains type name

namespace GitHub.Models
{
    public class UsageModel
    {
        public DimensionsModel Dimensions { get; set; } = new DimensionsModel();
        public MeasuresModel Measures { get; set; } = new MeasuresModel();

        // this should never be called by our code but it's required to be public by the serialization code
        public UsageModel() { }

        public static UsageModel Create(Guid guid)
        {
            return new UsageModel
            {
                Dimensions = new DimensionsModel
                {
                    Guid = guid,
                    Date = DateTimeOffset.Now,
                }
            };
        }

        public class DimensionsModel
        {
            public Guid Guid { get; set; }
            public DateTimeOffset Date { get; set; }
            public bool IsGitHubUser { get; set; }
            public bool IsEnterpriseUser { get; set; }
            public string AppVersion { get; set; }
            public string VSVersion { get; set; }
            public string Lang { get; set; }
            public string CurrentLang { get; set; }
            public string CurrentUILang { get; set; }
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
            public int NumberOfOAuthLogins { get; set; }
            public int NumberOfTokenLogins { get; set; }
            public int NumberOfPullRequestsOpened { get; set; }
            public int NumberOfLocalPullRequestsCheckedOut { get; set; }
            public int NumberOfLocalPullRequestPulls { get; set; }
            public int NumberOfLocalPullRequestPushes { get; set; }
            public int NumberOfForkPullRequestsCheckedOut { get; set; }
            public int NumberOfForkPullRequestPulls { get; set; }
            public int NumberOfForkPullRequestPushes { get; set; }
            public int NumberOfSyncSubmodules { get; set; }
            public int NumberOfWelcomeDocsClicks { get; set; }
            public int NumberOfWelcomeTrainingClicks { get; set; }
            public int NumberOfGitHubPaneHelpClicks { get; set; }
            public int NumberOfPRDetailsOpenInGitHub { get; set; }
            public int NumberOfPRStatusesOpenInGitHub { get; set; }
            public int NumberOfPRChecksOpenInGitHub { get; set; }
            public int NumberOfPRDetailsViewChanges { get; set; }
            public int NumberOfPRDetailsViewFile { get; set; }
            public int NumberOfPRDetailsCompareWithSolution { get; set; }
            public int NumberOfPRDetailsOpenFileInSolution { get; set; }
            public int NumberOfPRReviewDiffViewInlineCommentOpen { get; set; }
            public int NumberOfPRReviewDiffViewInlineCommentPost { get; set; }
            public int NumberOfPRReviewDiffViewInlineCommentDelete { get; set; }
            public int NumberOfPRReviewDiffViewInlineCommentEdit { get; set; }
            public int NumberOfPRReviewDiffViewInlineCommentStartReview { get; set; }
            public int NumberOfPRReviewPosts { get; set; }
            public int NumberOfShowCurrentPullRequest { get; set; }
            public int NumberOfStatusBarOpenPullRequestList { get; set; }
            public int NumberOfTeamExplorerHomeOpenPullRequestList { get; set; }
            public int NumberOfPullRequestOpenAnnotationsList { get; set; }
            public int NumberOfStartPageClones { get; set; }
            public int NumberOfGitHubConnectSectionClones { get; set; }
            public int NumberOfShowRepoForkDialogClicks { get; set; }
            public int NumberOfReposForked { get; set; }
            public int ExecuteGoToSolutionOrPullRequestFileCommand { get; set; }
            public int NumberOfPRDetailsNavigateToEditor { get; set; } // Should rename to NumberOfNavigateToEditor
            public int NumberOfNavigateToPullRequestFileDiff { get; set; }
            public int NumberOfNavigateToCodeView { get; set; }
            public int ExecuteToggleInlineCommentMarginCommand { get; set; }
            public int NumberOfPullRequestFileMarginToggleInlineCommentMargin { get; set; }
            public int NumberOfPullRequestFileMarginViewChanges { get; set; }
            public int NumberOfCloneViewGitHubTab { get; set; }
            public int NumberOfCloneViewEnterpriseTab { get; set; }
            public int NumberOfCloneViewUrlTab { get; set; }
            public int NumberOfGitHubClones { get; set; }
            public int NumberOfEnterpriseClones { get; set; }
            public int NumberOfGitHubOpens { get; set; }
            public int NumberOfEnterpriseOpens { get; set; }
            public int NumberOfClonesToDefaultClonePath { get; set; }
        }
    }
}
