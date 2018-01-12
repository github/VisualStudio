using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Api
{
    public interface IGraphQLClientFactory
    {
        Task<Octokit.GraphQL.Connection> CreateConnection(IConnection connection);
    }
}