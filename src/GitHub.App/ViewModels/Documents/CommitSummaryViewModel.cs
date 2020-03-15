using GitHub.Models;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// Displays a one-line summary of a commit in a pull request timeline.
    /// </summary>
    public class CommitSummaryViewModel : ViewModelBase, ICommitSummaryViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommitSummaryViewModel"/> class.
        /// </summary>
        /// <param name="commit">The commit model.</param>
        public CommitSummaryViewModel(CommitModel commit)
        {
            AbbreviatedOid = commit.AbbreviatedOid;
            Author = new CommitActorViewModel(commit.Author);
            Header = commit.MessageHeadline;
            Oid = commit.Oid;
        }

        /// <inheritdoc/>
        public string AbbreviatedOid { get; private set; }

        /// <inheritdoc/>
        public ICommitActorViewModel Author { get; private set; }

        /// <inheritdoc/>
        public string Header { get; private set; }

        /// <inheritdoc/>
        public string Oid { get; private set; }
    }
}
