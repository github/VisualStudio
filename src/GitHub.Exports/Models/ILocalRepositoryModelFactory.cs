namespace GitHub.Models
{
    /// <summary>
    /// A factory for <see cref="ILocalRepositoryModelFactory" /> objects.
    /// </summary>
    public interface ILocalRepositoryModelFactory
    {
        /// <summary>
        /// Construct a new <see cref="ILocalRepositoryModelFactory" />.
        /// </summary>
        /// <param name="localPath">The local path for the repository.</param>
        /// <returns>A new repository model.</returns>
        ILocalRepositoryModel Create(string localPath);
    }
}
