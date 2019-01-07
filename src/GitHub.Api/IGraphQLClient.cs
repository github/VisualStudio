using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;

namespace GitHub.Api
{
    public interface IGraphQLClient
    {
        Task<T> Run<T>(
            IQueryableValue<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> Run<T>(
            IQueryableList<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            CancellationToken cancellationToken = default);

        Task<T> Run<T>(
            ICompiledQuery<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            CancellationToken cancellationToken = default);
    }
}