using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.App;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Displays a short overview of a pull request review in the <see cref="PullRequestDetailViewModel"/>.
    /// </summary>
    public class PullRequestReviewSummaryViewModel : IPullRequestReviewSummaryViewModel
    {
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public IActorViewModel User { get; set; }

        /// <inheritdoc/>
        public PullRequestReviewState State { get; set; }

        /// <inheritdoc/>
        public string StateDisplay => ToString(State);

        /// <inheritdoc/>
        public int FileCommentCount { get; set; }

        /// <summary>
        /// Builds a collection of <see cref="PullRequestReviewSummaryViewModel"/>s by user.
        /// </summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="pullRequest">The pull request model.</param>
        /// <remarks>
        /// This method builds a list similar to that found in the "Reviewers" section at the top-
        /// right of the Pull Request page on GitHub.
        /// </remarks>
        public static IEnumerable<PullRequestReviewSummaryViewModel> BuildByUser(
            ActorModel currentUser,
            PullRequestDetailModel pullRequest)
        {
            var existing = new Dictionary<string, PullRequestReviewSummaryViewModel>();

            foreach (var review in pullRequest.Reviews.OrderBy(x => x.SubmittedAt))
            {
                if (review.State == PullRequestReviewState.Pending && review.Author.Login != currentUser.Login)
                    continue;

                PullRequestReviewSummaryViewModel previous;
                existing.TryGetValue(review.Author.Login, out previous);

                var previousPriority = ToPriority(previous);
                var reviewPriority = ToPriority(review.State);

                if (reviewPriority >= previousPriority)
                {
                    existing[review.Author.Login] = new PullRequestReviewSummaryViewModel
                    {
                        Id = review.Id,
                        User = new ActorViewModel(review.Author),
                        State = review.State,
                        FileCommentCount = review.Comments.Count,
                    };
                }
            }

            var result = existing.Values.OrderBy(x => x.User.Login).AsEnumerable();

            if (!result.Any(x => x.State == PullRequestReviewState.Pending))
            {
                var newReview = new PullRequestReviewSummaryViewModel
                {
                    State = PullRequestReviewState.Pending,
                    User = new ActorViewModel(currentUser),
                };
                result = result.Concat(new[] { newReview });
            }

            return result;
        }

        static int ToPriority(PullRequestReviewSummaryViewModel review)
        {
            return review != null ? ToPriority(review.State) : 0;
        }

        static int ToPriority(PullRequestReviewState state)
        {
            switch (state)
            {
                case PullRequestReviewState.Approved:
                case PullRequestReviewState.ChangesRequested:
                    return 1;
                case PullRequestReviewState.Pending:
                    return 2;
                default:
                    return 0;
            }
        }

        static string ToString(PullRequestReviewState state)
        {
            switch (state)
            {
                case PullRequestReviewState.Approved:
                    return Resources.Approved;
                case PullRequestReviewState.ChangesRequested:
                    return Resources.ChangesRequested;
                case PullRequestReviewState.Commented:
                case PullRequestReviewState.Dismissed:
                    return Resources.Commented;
                case PullRequestReviewState.Pending:
                    return Resources.InProgress;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
