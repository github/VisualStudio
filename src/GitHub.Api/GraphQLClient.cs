using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Octokit.GraphQL;
using Octokit.GraphQL.Core.Deserializers;

namespace GitHub.Api
{
    public class GraphQLClient : Connection, IGraphQLClient
    {
        readonly ObjectCache cache;
        readonly TimeSpan cacheDuration;
        readonly bool cachedReads;

        public GraphQLClient(
            ProductHeaderValue header,
            ICredentialStore credentialStore,
            ObjectCache cache,
            TimeSpan cacheDuration)
            : this(header, credentialStore, cache, cacheDuration, true)
        {
        }

        private GraphQLClient(
            ProductHeaderValue header,
            ICredentialStore credentialStore,
            ObjectCache cache,
            TimeSpan cacheDuration,
            bool cachedReads)
            : base(header, credentialStore)
        {
            this.cache = cache;
            this.cacheDuration = cacheDuration;
            this.cachedReads = cachedReads;

            if (cachedReads)
            {
                NonCached = new GraphQLClient(header, credentialStore, cache, cacheDuration, false);
            }
        }

        public IConnection NonCached { get; }

        public override async Task<T> Run<T>(CompiledQuery<T> query, IDictionary<string, object> variables = null)
        {
            var deserializer = new ResponseDeserializer();
            var payload = query.GetPayload(variables);
            var hash = GetHash(payload);
            var data = cachedReads ? (string)cache.Get(hash, Uri.Host) : null;

            if (data == null)
            {
                data = await Send(payload);
                var result = deserializer.Deserialize(query, data);
                cache.Add(hash, data, DateTimeOffset.Now + cacheDuration, Uri.Host);
                return result;
            }
            else
            {
                return deserializer.Deserialize(query, data);
            }
        }

        static string GetHash(string input)
        {
            var sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                foreach (var b in result) sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
