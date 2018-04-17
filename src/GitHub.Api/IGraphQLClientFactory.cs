using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Api
{
    /// <summary>
    /// Creates GraphQL <see cref="Octokit.GraphQL.IConnection"/>s for querying the
    /// GitHub GraphQL API.
    /// </summary>
    public interface IGraphQLClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="Octokit.GraphQL.IConnection"/>.
        /// </summary>
        /// <param name="address">The address of the server.</param>
        /// <returns>A task returning the created connection.</returns>
        Task<Octokit.GraphQL.IConnection> CreateConnection(HostAddress address);
    }
}