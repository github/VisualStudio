using System;
using System.Diagnostics;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
using Serilog;
using static System.FormattableString;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryHost : ReactiveObject, IRepositoryHost
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryHosts>();

        readonly IConnection connection;

        public RepositoryHost(
            IConnection connection,
            IApiClient apiClient,
            IModelService modelService)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));
            Guard.ArgumentNotNull(apiClient, nameof(apiClient));
            Guard.ArgumentNotNull(modelService, nameof(modelService));

            this.connection = connection;
            ApiClient = apiClient;
            ModelService = modelService;
        }

        public HostAddress Address => ApiClient.HostAddress;
        public IApiClient ApiClient { get;}
        public bool IsLoggedIn => connection.IsLoggedIn;
        public IModelService ModelService { get; }
        public string Title => ApiClient.HostAddress.Title;

        internal string DebuggerDisplay => Invariant($"RepositoryHost: {Title} {Address.ApiUri}");
    }
}
