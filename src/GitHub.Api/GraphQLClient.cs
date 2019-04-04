using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;

namespace GitHub.Api
{
    public class GraphQLClient : IGraphQLClient
    {
        public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(8);
        readonly IConnection connection;
        readonly FileCache cache;

        public GraphQLClient(
            IConnection connection,
            FileCache cache)
        {
            this.connection = connection;
            this.cache = cache;
        }

        public Task ClearCache(string regionName)
        {
            // Switch to background thread because FileCache does not provide an async API.
            return Task.Run(() => cache.ClearRegion(GetFullRegionName(regionName)));
        }

        public Task<T> Run<T>(
            IQueryableValue<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            string regionName = null,
            CancellationToken cancellationToken = default)
        {
            return Run(query.Compile(), variables, refresh, cacheDuration, regionName, cancellationToken);
        }

        public Task<IEnumerable<T>> Run<T>(
            IQueryableList<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            string regionName = null,
            CancellationToken cancellationToken = default)
        {
            return Run(query.Compile(), variables, refresh, cacheDuration, regionName, cancellationToken);
        }

        public async Task<T> Run<T>(
            ICompiledQuery<T> query,
            Dictionary<string, object> variables = null,
            bool refresh = false,
            TimeSpan? cacheDuration = null,
            string regionName = null,
            CancellationToken cancellationToken = default)
        {
            if (!query.IsMutation)
            {
                var wrapper = new CachingWrapper(
                    this,
                    refresh,
                    cacheDuration ?? DefaultCacheDuration,
                    GetFullRegionName(regionName));
                return await wrapper.Run(query, variables, cancellationToken);
            }
            else
            {
                return await connection.Run(query, variables, cancellationToken);
            }
        }

        string GetFullRegionName(string regionName)
        {
            var result = connection.Uri.Host;

            if (!string.IsNullOrWhiteSpace(regionName))
            {
                result += Path.DirectorySeparatorChar + regionName;
            }

            return result.EnsureValidPath();
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
            readonly bool refresh;
            readonly TimeSpan cacheDuration;
            readonly string regionName;

            public CachingWrapper(
                GraphQLClient owner,
                bool refresh,
                TimeSpan cacheDuration,
                string regionName)
            {
                this.owner = owner;
                this.refresh = refresh;
                this.cacheDuration = cacheDuration;
                this.regionName = regionName;
            }

            public Uri Uri => owner.connection.Uri;

            public Task<string> Run(string query, CancellationToken cancellationToken = default)
            {
                // Switch to background thread because FileCache does not provide an async API.
                return Task.Run(async () =>
                {
                    var hash = GetHash(query);

                    if (refresh)
                    {
                        owner.cache.Remove(hash, regionName);
                    }

                    var data = (string) owner.cache.Get(hash, regionName);

                    if (data != null)
                    {
                        return data;
                    }

                    var result = await owner.connection.Run(query, cancellationToken);
                    owner.cache.Add(hash, result, DateTimeOffset.Now + cacheDuration, regionName);
                    return result;
                }, cancellationToken);
            }
        }
    }
}
