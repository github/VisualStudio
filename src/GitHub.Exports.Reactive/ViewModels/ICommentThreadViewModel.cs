using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A comment thread.
    /// </summary>
    public interface ICommentThreadViewModel
    {
        /// <summary>
        /// Gets the comments in the thread.
        /// </summary>
        ObservableCollection<ICommentViewModel> Comments { get; }

        /// <summary>
        /// Gets the current user under whos account new comments will be created.
        /// </summary>
        IActorViewModel CurrentUser { get; }

        /// <summary>
        /// Called by a comment in the thread to post itself as a new comment to the API.
        /// </summary>
        ReactiveCommand<string, Unit> PostComment { get; }

        /// <summary>
        /// Called by a comment in the thread to post itself as an edit to a comment to the API.
        /// </summary>
        ReactiveCommand<Tuple<string, string>, Unit> EditComment { get; }

        /// <summary>
        /// Called by a comment in the thread to send a delete of the comment to the API.
        /// </summary>
        ReactiveCommand<Tuple<int, int>, Unit> DeleteComment { get; }
    }
}
