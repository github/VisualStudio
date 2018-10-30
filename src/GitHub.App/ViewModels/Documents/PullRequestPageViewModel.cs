using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.SampleData;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for displaying a pull request in a document window.
    /// </summary>
    [Export(typeof(IPullRequestPageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestPageViewModel : PullRequestViewModelBase, IPullRequestPageViewModel
    {
        readonly IViewViewModelFactory factory;
        readonly IPullRequestSessionManager sessionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestPageViewModel"/> class.
        /// </summary>
        /// <param name="factory">The view model factory.</param>
        [ImportingConstructor]
        public PullRequestPageViewModel(
            IViewViewModelFactory factory,
            IPullRequestSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.factory = factory;
            this.sessionManager = sessionManager;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IViewModel> Timeline { get; private set; }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            ActorModel currentUser,
            PullRequestDetailModel model)
        {
            await base.InitializeAsync(model).ConfigureAwait(true);

            Timeline = new IViewModel[]
            {
                new CommitSummariesViewModel(
                    new CommitSummaryViewModel(new CommitModel
                    {
                        Author = new ActorModel { Login = "grokys" },
                        AbbreviatedOid = "c7c7d25",
                        MessageHeadline = "Refactor comment view models."
                    }),
                    new CommitSummaryViewModel(new CommitModel
                    {
                        Author = new ActorModel { Login = "grokys" },
                        AbbreviatedOid = "04e6a90",
                        MessageHeadline = "Refactor comment view models.",
                    })),
                new CommentViewModelDesigner
                {
                    Author = new ActorViewModelDesigner("meaghanlewis"),
                    Body = @"This is looking great! Really enjoying using this feature so far.

When leaving an inline comment, the comment posts successfully and then a new comment is drafted with the same text.",
                },
                new CommentViewModelDesigner
                {
                    Author = new ActorViewModelDesigner("grokys"),
                    Body = @"Oops, sorry about that @meaghanlewis - I was sure I tested those things, but must have got messed up again at some point. Should be fixed now.",
                },
            };
        }
    }
}
