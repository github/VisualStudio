using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using Microsoft.VisualStudio.Shell;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using Rothko;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    /// <summary>
    /// Service used to clone GitHub repositories. It wraps the
    /// <see cref="Microsoft.TeamFoundation.Git.Controls.Extensibility.IGitRepositoriesExt"/> service provided
    /// by Team Explorer.
    /// </summary>
    [Export(typeof(IRepositoryCloneService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCloneService : IRepositoryCloneService
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryCloneService>();

        readonly IOperatingSystem operatingSystem;
        readonly string defaultClonePath;
        readonly IVSGitServices vsGitServices;
        readonly ITeamExplorerServices teamExplorerServices;
        readonly IGraphQLClientFactory graphqlFactory;
        readonly IGitHubContextService gitHubContextService;
        readonly IUsageTracker usageTracker;
        readonly Lazy<EnvDTE.DTE> dte;
        ICompiledQuery<ViewerRepositoriesModel> readViewerRepositories;

        [ImportingConstructor]
        public RepositoryCloneService(
            IOperatingSystem operatingSystem,
            IVSGitServices vsGitServices,
            ITeamExplorerServices teamExplorerServices,
            IGraphQLClientFactory graphqlFactory,
            IGitHubContextService gitHubContextService,
            IUsageTracker usageTracker,
            IGitHubServiceProvider sp)
        {
            this.operatingSystem = operatingSystem;
            this.vsGitServices = vsGitServices;
            this.teamExplorerServices = teamExplorerServices;
            this.graphqlFactory = graphqlFactory;
            this.gitHubContextService = gitHubContextService;
            this.usageTracker = usageTracker;
            dte = new Lazy<EnvDTE.DTE>(() => sp.GetService<EnvDTE.DTE>());

            defaultClonePath = GetLocalClonePathFromGitProvider(operatingSystem.Environment.GetUserRepositoriesPath());
        }

        /// <inheritdoc/>
        public async Task<ViewerRepositoriesModel> ReadViewerRepositories(HostAddress address)
        {
            if (readViewerRepositories == null)
            {
                var order = new RepositoryOrder
                {
                    Field = RepositoryOrderField.PushedAt,
                    Direction = OrderDirection.Desc
                };

                var repositorySelection = new Fragment<Repository, RepositoryListItemModel>(
                    "repository",
                    repo => new RepositoryListItemModel
                    {
                        IsFork = repo.IsFork,
                        IsPrivate = repo.IsPrivate,
                        Name = repo.Name,
                        Owner = repo.Owner.Login,
                        Url = new Uri(repo.Url),
                    });

                readViewerRepositories = new Query()
                    .Viewer
                    .Select(viewer => new ViewerRepositoriesModel
                    {
                        Owner = viewer.Login,
                        Repositories = viewer.Repositories(null, null, null, null, null, null, null, order, null, null)
                            .AllPages()
                            .Select(repositorySelection).ToList(),
                        ContributedToRepositories = viewer.RepositoriesContributedTo(100, null, null, null, null, null, null, order, null)
                            .Nodes
                            .Select(repositorySelection).ToList(),
                        Organizations = viewer.Organizations(null, null, null, null).AllPages().Select(org => new
                        {
                            org.Login,
                            Repositories = org.Repositories(100, null, null, null, null, null, null, order, null, null)
                                .Nodes
                                .Select(repositorySelection).ToList()
                        }).ToDictionary(x => x.Login, x => (IReadOnlyList<RepositoryListItemModel>)x.Repositories),
                    }).Compile();
            }

            var graphql = await graphqlFactory.CreateConnection(address).ConfigureAwait(false);
            var result = await graphql.Run(readViewerRepositories).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task CloneOrOpenRepository(
            CloneDialogResult cloneDialogResult,
            object progress = null)
        {
            Guard.ArgumentNotNull(cloneDialogResult, nameof(cloneDialogResult));

            var repositoryPath = cloneDialogResult.Path;
            var url = cloneDialogResult.Url;

            if (DestinationFileExists(repositoryPath))
            {
                throw new InvalidOperationException("Can't clone or open a repository because a file exists at: " + repositoryPath);
            }

            var repositoryUrl = url.ToRepositoryUrl();
            var isDotCom = HostAddress.IsGitHubDotComUri(repositoryUrl);
            if (DestinationDirectoryExists(repositoryPath))
            {
                if (!IsSolutionInRepository(repositoryPath))
                {
                    teamExplorerServices.OpenRepository(repositoryPath);
                }

                if (isDotCom)
                {
                    await usageTracker.IncrementCounter(x => x.NumberOfGitHubOpens);
                }
                else
                {
                    await usageTracker.IncrementCounter(x => x.NumberOfEnterpriseOpens);
                }
            }
            else
            {
                var cloneUrl = repositoryUrl.ToString();
                await CloneRepository(cloneUrl, repositoryPath, progress).ConfigureAwait(true);

                if (isDotCom)
                {
                    await usageTracker.IncrementCounter(x => x.NumberOfGitHubClones);
                }
                else
                {
                    await usageTracker.IncrementCounter(x => x.NumberOfEnterpriseClones);
                }
            }

            // Give user a chance to choose a solution
            teamExplorerServices.ShowHomePage();

            // Navigate to context for supported URL types (e.g. /blob/ URLs)
            var context = gitHubContextService.FindContextFromUrl(url);
            if (context != null)
            {
                gitHubContextService.TryNavigateToContext(repositoryPath, context);
            }
        }

        bool IsSolutionInRepository(string repositoryPath)
        {
            var solutionPath = dte.Value.Solution.FileName;
            if (string.IsNullOrEmpty(solutionPath))
            {
                return false;
            }

            var isFolder = operatingSystem.Directory.DirectoryExists(solutionPath);
            var solutionDir = isFolder ? solutionPath : Path.GetDirectoryName(solutionPath);
            if (string.Equals(repositoryPath, solutionDir, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (solutionDir.StartsWith(repositoryPath + '\\', StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task CloneRepository(
            string cloneUrl,
            string repositoryPath,
            object progress = null)
        {
            Guard.ArgumentNotEmptyString(cloneUrl, nameof(cloneUrl));
            Guard.ArgumentNotEmptyString(repositoryPath, nameof(repositoryPath));

            // Switch to a thread pool thread for IO then back to the main thread to call
            // vsGitServices.Clone() as this must be called on the main thread.
            await ThreadingHelper.SwitchToPoolThreadAsync();
            operatingSystem.Directory.CreateDirectory(repositoryPath);
            await ThreadingHelper.SwitchToMainThreadAsync();

            try
            {
                await vsGitServices.Clone(cloneUrl, repositoryPath, true, progress);
                await usageTracker.IncrementCounter(x => x.NumberOfClones);

                if (repositoryPath.StartsWith(DefaultClonePath, StringComparison.OrdinalIgnoreCase))
                {
                    // Count the number of times users clone into the Default Repository Location
                    await usageTracker.IncrementCounter(x => x.NumberOfClonesToDefaultClonePath);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Could not clone {CloneUrl} to {Path}", cloneUrl, repositoryPath);
                throw;
            }
        }

        /// <inheritdoc/>
        public bool DestinationDirectoryExists(string path) => operatingSystem.Directory.DirectoryExists(path);

        /// <inheritdoc/>
        public bool DestinationFileExists(string path) => operatingSystem.File.Exists(path);

        string GetLocalClonePathFromGitProvider(string fallbackPath)
        {
            var ret = vsGitServices.GetLocalClonePathFromGitProvider();
            return !string.IsNullOrEmpty(ret)
                ? operatingSystem.Environment.ExpandEnvironmentVariables(ret)
                : fallbackPath;
        }

        public string DefaultClonePath { get { return defaultClonePath; } }

        class OrganizationAdapter
        {
            public IReadOnlyList<RepositoryListItemModel> Repositories { get; set; }
        }
    }
}
