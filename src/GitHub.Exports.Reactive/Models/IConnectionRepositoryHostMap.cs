namespace GitHub.Models
{
    /// <summary>
    /// Used to associate the current connection to its related repository host.
    /// </summary>
    public interface IConnectionRepositoryHostMap
    {
        /// <summary>
        /// The current repository host associated with the current action.
        /// </summary>
        IRepositoryHost CurrentRepositoryHost { get; }
    }
}
