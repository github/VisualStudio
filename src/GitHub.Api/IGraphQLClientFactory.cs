using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Api
{
    public interface IGraphQLClientFactory
    {
        Task<Octokit.GraphQL.IConnection> CreateConnection(HostAddress address);
    }
}