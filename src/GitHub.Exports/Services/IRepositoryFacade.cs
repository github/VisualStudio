using LibGit2Sharp;

namespace GitHub.Services
{
    /// <summary>
    /// Facade for <see cref="LibGit2Sharp.Repository"/> static methods.
    /// </summary>
    public interface IRepositoryFacade
    {
        IRepository NewRepository(string path);
        string Discover(string startingPath);
    }
}