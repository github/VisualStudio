namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// Displays a one-line summary of a commit in a pull request timeline.
    /// </summary>
    public interface ICommitSummaryViewModel : IViewModel
    {
        /// <summary>
        /// Gets the abbreviated OID (SHA) of the commit.
        /// </summary>
        string AbbreviatedOid { get; }

        /// <summary>
        /// Gets the commit author.
        /// </summary>
        ICommitActorViewModel Author { get; }

        /// <summary>
        /// Gets the commit message header.
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Gets the OID (SHA) of the commit.
        /// </summary>
        string Oid { get; }
    }
}