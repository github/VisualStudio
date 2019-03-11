using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Api
{
    /// <summary>
    /// Creates <see cref="IGraphQLClient"/>s for querying the GitHub GraphQL API.
    /// </summary>
    public interface IGraphQLClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="IGraphQLClient"/>.
        /// </summary>
        /// <param name="address">The address of the server.</param>
        /// <returns>A task returning the created client.</returns>
        Task<IGraphQLClient> CreateConnection(HostAddress address);
    }
}