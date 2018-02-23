using System;
using Octokit.GraphQL;

namespace GitHub.Api
{
    public interface IGraphQLClient : IConnection
    {
        IConnection NonCached { get; }
    }
}
