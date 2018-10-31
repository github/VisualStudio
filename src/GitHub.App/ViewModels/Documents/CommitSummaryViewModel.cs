using GitHub.Models;

namespace GitHub.ViewModels.Documents
{
    public class CommitSummaryViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommitSummaryViewModel"/> class.
        /// </summary>
        /// <param name="commit">The commit model.</param>
        public CommitSummaryViewModel(CommitModel commit)
        {
            AbbreviatedOid = commit.AbbreviatedOid;
            Author = new ActorViewModel(commit.Author);
            Header = commit.MessageHeadline;
            Oid = commit.Oid;
        }

        /// <inheritdoc/>
        public string AbbreviatedOid { get; private set; }

        /// <inheritdoc/>
        public IActorViewModel Author { get; private set; }

        /// <inheritdoc/>
        public string Header { get; private set; }

        /// <inheritdoc/>
        public string Oid { get; private set; }
    }
}
