using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;

namespace GitHub.Api
{
    public class GraphQLClient : IGraphQLClient
    {
        public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(8);
        readonly IConnection connection;
        readonly ObjectCache cache;

        public GraphQLClient(
            IConnection connection,
            ObjectCache cache)
        {
            this.connection = connection;
            this.cache = cache;
        }

        public Task<T> Run<T>(
            IQueryableValue<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            CancellationToken cancellationToken = default)
        {
            return Run(query.Compile(), variables, refresh, cacheDuration, cancellationToken);
        }

        public Task<IEnumerable<T>> Run<T>(
            IQueryableList<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            CancellationToken cancellationToken = default)
        {
            return Run(query.Compile(), variables, refresh, cacheDuration, cancellationToken);
        }

        public async Task<T> Run<T>(
            ICompiledQuery<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            CancellationToken cancellationToken = default)
        {
            if (!query.IsMutation)
            {
                var wrapper = new CachingWrapper(this, cacheDuration ?? DefaultCacheDuration, refresh);
                return await wrapper.Run(query, variables, cancellationToken);
            }
            else
            {
                return await connection.Run(query, variables, cancellationToken);
            }
        }

        static string GetHash(string input)
        {
            var sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                foreach (var b in result)
                {
                    sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }
            }

            return sb.ToString();
        }

        class CachingWrapper : IConnection
        {
            readonly GraphQLClient owner;
            readonly TimeSpan cacheDuration;
            readonly bool refresh;

            public CachingWrapper(
                GraphQLClient owner,
                TimeSpan cacheDuration,
                bool refresh)
            {
                this.owner = owner;
                this.cacheDuration = cacheDuration;
                this.refresh = refresh;
            }

            public Uri Uri => owner.connection.Uri;

            public async Task<string> Run(string query, CancellationToken cancellationToken = default)
            {
                // Switch to background thread because ObjectCache does not provide an async API.
                await TaskScheduler.Default;

                var hash = GetHash(query);

                if (refresh)
                {
                    owner.cache.Remove(hash, Uri.Host);
                }

                var data = (string)owner.cache.Get(hash, Uri.Host);

                if (data != null)
                {
                    return data;
                }

                var result = await owner.connection.Run(query, cancellationToken);
                owner.cache.Add(hash, result, DateTimeOffset.Now + cacheDuration, Uri.Host);
                return result;
            }
        }
    }
}
