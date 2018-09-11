using System;
using GitHub.Models;
using Octokit.GraphQL.Model;
using CheckAnnotationLevel = GitHub.Models.CheckAnnotationLevel;
using CheckConclusionState = GitHub.Models.CheckConclusionState;
using CheckStatusState = GitHub.Models.CheckStatusState;
using PullRequestReviewState = GitHub.Models.PullRequestReviewState;
using StatusState = GitHub.Models.StatusState;

namespace GitHub.Services
{
    public static class FromGraphQlExtensions
    {
        public static CheckConclusionState? FromGraphQl(this Octokit.GraphQL.Model.CheckConclusionState? value)
        {
            switch (value)
            {
                case null:
                    return null;
                case Octokit.GraphQL.Model.CheckConclusionState.ActionRequired:
                    return CheckConclusionState.ActionRequired;
                case Octokit.GraphQL.Model.CheckConclusionState.TimedOut:
                    return CheckConclusionState.TimedOut;
                case Octokit.GraphQL.Model.CheckConclusionState.Cancelled:
                    return CheckConclusionState.Cancelled;
                case Octokit.GraphQL.Model.CheckConclusionState.Failure:
                    return CheckConclusionState.Failure;
                case Octokit.GraphQL.Model.CheckConclusionState.Success:
                    return CheckConclusionState.Success;
                case Octokit.GraphQL.Model.CheckConclusionState.Neutral:
                    return CheckConclusionState.Neutral;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static PullRequestStateEnum FromGraphQl(this PullRequestState value)
        {
            switch (value)
            {
                case PullRequestState.Open:
                    return PullRequestStateEnum.Open;
                case PullRequestState.Closed:
                    return PullRequestStateEnum.Closed;
                case PullRequestState.Merged:
                    return PullRequestStateEnum.Merged;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static StatusState FromGraphQl(this Octokit.GraphQL.Model.StatusState value)
        {
            switch (value)
            {
                case Octokit.GraphQL.Model.StatusState.Expected:
                    return StatusState.Expected;
                case Octokit.GraphQL.Model.StatusState.Error:
                    return StatusState.Error;
                case Octokit.GraphQL.Model.StatusState.Failure:
                    return StatusState.Failure;
                case Octokit.GraphQL.Model.StatusState.Pending:
                    return StatusState.Pending;
                case Octokit.GraphQL.Model.StatusState.Success:
                    return StatusState.Success;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static CheckStatusState FromGraphQl(this Octokit.GraphQL.Model.CheckStatusState value)
        {
            switch (value)
            {
                case Octokit.GraphQL.Model.CheckStatusState.Queued:
                    return CheckStatusState.Queued;
                case Octokit.GraphQL.Model.CheckStatusState.InProgress:
                    return CheckStatusState.InProgress;
                case Octokit.GraphQL.Model.CheckStatusState.Completed:
                    return CheckStatusState.Completed;
                case Octokit.GraphQL.Model.CheckStatusState.Requested:
                    return CheckStatusState.Requested;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static PullRequestReviewState FromGraphQl(this Octokit.GraphQL.Model.PullRequestReviewState value)
        {
            switch (value) {
                case Octokit.GraphQL.Model.PullRequestReviewState.Pending:
                    return PullRequestReviewState.Pending;
                case Octokit.GraphQL.Model.PullRequestReviewState.Commented:
                    return PullRequestReviewState.Commented;
                case Octokit.GraphQL.Model.PullRequestReviewState.Approved:
                    return PullRequestReviewState.Approved;
                case Octokit.GraphQL.Model.PullRequestReviewState.ChangesRequested:
                    return PullRequestReviewState.ChangesRequested;
                case Octokit.GraphQL.Model.PullRequestReviewState.Dismissed:
                    return PullRequestReviewState.Dismissed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static CheckAnnotationLevel FromGraphQl(this Octokit.GraphQL.Model.CheckAnnotationLevel value)
        {
            switch (value)
            {
                case Octokit.GraphQL.Model.CheckAnnotationLevel.Failure:
                    return CheckAnnotationLevel.Failure;
                case Octokit.GraphQL.Model.CheckAnnotationLevel.Notice:
                    return CheckAnnotationLevel.Notice;
                case Octokit.GraphQL.Model.CheckAnnotationLevel.Warning:
                    return CheckAnnotationLevel.Warning;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}