using System.ComponentModel.Composition;
using LibGit2Sharp;

namespace GitHub.Services
{
    /// <summary>
    /// Facade for <see cref="LibGit2Sharp.Repository"/> static methods.
    /// </summary>
    [Export(typeof(IRepositoryFacade))]
    public class RepositoryFacade : IRepositoryFacade
    {
        public IRepository NewRepository(string path)
        {
            return new Repository(path);
        }

        public string Discover(string startingPath)
        {
            return Repository.Discover(startingPath);
        }
    }
}