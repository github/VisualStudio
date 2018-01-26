using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Api
{
    public interface IGraphQLClientFactory
    {
        Task<Octokit.GraphQL.Connection> CreateConnection(HostAddress address);
    }
}