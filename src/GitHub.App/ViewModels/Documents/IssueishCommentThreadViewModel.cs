using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// A thread of issue or pull request comments.
    /// </summary>
    [Export(typeof(IIssueishCommentThreadViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueishCommentThreadViewModel : CommentThreadViewModel, IIssueishCommentThreadViewModel
    {
        readonly IViewViewModelFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueishCommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="factory">The view model factory.</param>
        [ImportingConstructor]
        public IssueishCommentThreadViewModel(IViewViewModelFactory factory)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));
            this.factory = factory;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            ActorModel currentUser,
            IssueishDetailModel model,
            bool addPlaceholder)
        {
            Guard.ArgumentNotNull(model, nameof(model));

            await base.InitializeAsync(currentUser).ConfigureAwait(false);

            foreach (var comment in model.Comments)
            {
                var vm = factory.CreateViewModel<ICommentViewModel>();
                await vm.InitializeAsync(
                    this,
                    currentUser,
                    comment,
                    CommentEditState.None).ConfigureAwait(true);
                Comments.Add(vm);
            }

            if (addPlaceholder)
            {
                var vm = factory.CreateViewModel<ICommentViewModel>();
                await vm.InitializeAsync(
                    this,
                    currentUser,
                    null,
                    CommentEditState.Placeholder).ConfigureAwait(true);
                Comments.Add(vm);
            }
        }

        /// <inheritdoc/>
        public override Task PostComment(string body)
        {
            Guard.ArgumentNotNull(body, nameof(body));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Task EditComment(string id, string body)
        {
            Guard.ArgumentNotNull(id, nameof(id));
            Guard.ArgumentNotNull(body, nameof(body));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Task DeleteComment(int pullRequestId, int commentId)
        {
            throw new NotImplementedException();
        }
    }
}
