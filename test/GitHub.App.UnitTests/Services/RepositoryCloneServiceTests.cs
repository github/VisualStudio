using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using NUnit.Framework;
using Rothko;

public class RepositoryCloneServiceTests
{
    public class TheCloneRepositoryMethod
    {
        [Test]
        public async Task ClonesToRepositoryPathAsync()
        {
            var operatingSystem = Substitute.For<IOperatingSystem>();
            var vsGitServices = Substitute.For<IVSGitServices>();
            var cloneService = CreateRepositoryCloneService(operatingSystem, vsGitServices);

            await cloneService.CloneRepository("https://github.com/foo/bar", @"c:\dev\bar");

            operatingSystem.Directory.Received().CreateDirectory(@"c:\dev\bar");
            await vsGitServices.Received().Clone("https://github.com/foo/bar", @"c:\dev\bar", true);
        }

        [TestCase("https://github.com/foo/bar", 1, nameof(UsageModel.MeasuresModel.NumberOfClones))]
        [TestCase("https://github.com/foo/bar", 0, nameof(UsageModel.MeasuresModel.NumberOfGitHubClones))]
        [TestCase("https://github.com/foo/bar", 0, nameof(UsageModel.MeasuresModel.NumberOfEnterpriseClones))]
        [TestCase("https://enterprise.com/foo/bar", 1, nameof(UsageModel.MeasuresModel.NumberOfClones))]
        [TestCase("https://enterprise.com/foo/bar", 0, nameof(UsageModel.MeasuresModel.NumberOfEnterpriseClones))]
        [TestCase("https://enterprise.com/foo/bar", 0, nameof(UsageModel.MeasuresModel.NumberOfGitHubClones))]
        public async Task UpdatesMetricsWhenRepositoryClonedAsync(string cloneUrl, int numberOfCalls, string counterName)
        {
            var vsGitServices = Substitute.For<IVSGitServices>();
            var usageTracker = Substitute.For<IUsageTracker>();
            var cloneService = CreateRepositoryCloneService(vsGitServices: vsGitServices, usageTracker: usageTracker);

            await cloneService.CloneRepository(cloneUrl, @"c:\dev\bar");
            var model = UsageModel.Create(Guid.NewGuid());

            await usageTracker.Received(numberOfCalls).IncrementCounter(
                Arg.Is<Expression<Func<UsageModel.MeasuresModel, int>>>(x =>
                    ((MemberExpression)x.Body).Member.Name == counterName));
        }

        [TestCase(@"c:\repository", "", true, 1)]
        [TestCase(@"c:\repository", @"c:\solution", true, 1)]
        [TestCase(@"c:\already\open", @"c:\already\open", true, 0)]
        [TestCase(@"c:\already\open", @"c:\already\open\nested", true, 0, Description = "Solution folder in repository")]
        [TestCase(@"c:\already\open", @"c:\already\open\my.sln", false, 0)]
        [TestCase(@"c:\already\open", @"c:\already\open\nested\my.sln", false, 0)]
        [TestCase(@"c:\already\open\nested", @"c:\already\open", true, 1, Description = "Repository in solution folder")]
        public async Task Skip_OpenRepository_When_Already_Open(string repositoryPath, string solutionPath,
            bool isFolder, int openRepository)
        {
            var repositoryUrl = "https://github.com/owner/repo";
            var cloneDialogResult = new CloneDialogResult(repositoryPath, repositoryUrl);
            var operatingSystem = Substitute.For<IOperatingSystem>();
            var serviceProvider = Substitute.For<IGitHubServiceProvider>();
            var teamExplorerServices = Substitute.For<ITeamExplorerServices>();
            operatingSystem.Directory.DirectoryExists(repositoryPath).Returns(true);
            var dte = Substitute.For<EnvDTE.DTE>();
            serviceProvider.GetService<EnvDTE.DTE>().Returns(dte);
            dte.Solution.FileName.Returns(solutionPath);
            if (isFolder)
            {
                operatingSystem.Directory.DirectoryExists(solutionPath).Returns(true);
            }
            var cloneService = CreateRepositoryCloneService(operatingSystem: operatingSystem,
                teamExplorerServices: teamExplorerServices, serviceProvider: serviceProvider);

            await cloneService.CloneOrOpenRepository(cloneDialogResult);

            teamExplorerServices.Received(openRepository).OpenRepository(repositoryPath);
        }

        [TestCase("https://github.com/foo/bar", false, 1, nameof(UsageModel.MeasuresModel.NumberOfClones))]
        [TestCase("https://github.com/foo/bar", false, 1, nameof(UsageModel.MeasuresModel.NumberOfGitHubClones))]
        [TestCase("https://github.com/foo/bar", false, 0, nameof(UsageModel.MeasuresModel.NumberOfEnterpriseClones))]
        [TestCase("https://enterprise.com/foo/bar", false, 1, nameof(UsageModel.MeasuresModel.NumberOfClones))]
        [TestCase("https://enterprise.com/foo/bar", false, 1, nameof(UsageModel.MeasuresModel.NumberOfEnterpriseClones))]
        [TestCase("https://enterprise.com/foo/bar", false, 0, nameof(UsageModel.MeasuresModel.NumberOfGitHubClones))]

        [TestCase("https://github.com/foo/bar", true, 1, nameof(UsageModel.MeasuresModel.NumberOfGitHubOpens))]
        [TestCase("https://github.com/foo/bar", true, 0, nameof(UsageModel.MeasuresModel.NumberOfEnterpriseOpens))]
        [TestCase("https://enterprise.com/foo/bar", true, 1, nameof(UsageModel.MeasuresModel.NumberOfEnterpriseOpens))]
        [TestCase("https://enterprise.com/foo/bar", true, 0, nameof(UsageModel.MeasuresModel.NumberOfGitHubOpens))]
        public async Task UpdatesMetricsWhenCloneOrOpenRepositoryAsync(string cloneUrl, bool dirExists, int numberOfCalls, string counterName)
        {
            var repositoryPath = @"c:\dev\bar";
            var cloneDialogResult = new CloneDialogResult(repositoryPath, cloneUrl);
            var operatingSystem = Substitute.For<IOperatingSystem>();
            var usageTracker = Substitute.For<IUsageTracker>();
            operatingSystem.Directory.DirectoryExists(repositoryPath).Returns(dirExists);
            var cloneService = CreateRepositoryCloneService(operatingSystem: operatingSystem, usageTracker: usageTracker);

            await cloneService.CloneOrOpenRepository(cloneDialogResult);

            await usageTracker.Received(numberOfCalls).IncrementCounter(
                Arg.Is<Expression<Func<UsageModel.MeasuresModel, int>>>(x =>
                    ((MemberExpression)x.Body).Member.Name == counterName));
        }

        [TestCase(@"c:\default\repo", @"c:\default", 1, nameof(UsageModel.MeasuresModel.NumberOfClonesToDefaultClonePath))]
        [TestCase(@"c:\not_default\repo", @"c:\default", 0, nameof(UsageModel.MeasuresModel.NumberOfClonesToDefaultClonePath))]
        public async Task UpdatesMetricsWhenDefaultClonePath(string targetPath, string defaultPath, int numberOfCalls, string counterName)
        {
            var vsGitServices = Substitute.For<IVSGitServices>();
            vsGitServices.GetLocalClonePathFromGitProvider().Returns(defaultPath);
            var usageTracker = Substitute.For<IUsageTracker>();
            var cloneService = CreateRepositoryCloneService(usageTracker: usageTracker, vsGitServices: vsGitServices);

            await cloneService.CloneRepository("https://github.com/foo/bar", targetPath);
            var model = UsageModel.Create(Guid.NewGuid());

            await usageTracker.Received(numberOfCalls).IncrementCounter(
                Arg.Is<Expression<Func<UsageModel.MeasuresModel, int>>>(x =>
                    ((MemberExpression)x.Body).Member.Name == counterName));
        }

        [Test]
        public async Task CleansDirectoryOnCloneFailed()
        {
            var cloneUrl = "https://github.com/failing/url";
            var clonePath = @"c:\dev\bar";
            var operatingSystem = Substitute.For<IOperatingSystem>();
            var vsGitServices = Substitute.For<IVSGitServices>();
            vsGitServices.Clone(cloneUrl, clonePath, true).Returns(x => { throw new Exception(); });
            var cloneService = CreateRepositoryCloneService(operatingSystem: operatingSystem, vsGitServices: vsGitServices);

            Assert.ThrowsAsync<Exception>(() => cloneService.CloneRepository(cloneUrl, clonePath));

            operatingSystem.Directory.Received().CreateDirectory(clonePath);
            operatingSystem.Directory.Received().DeleteDirectory(clonePath);
            await vsGitServices.Received().Clone(cloneUrl, clonePath, true);
        }

        [Test]
        public async Task CloneIntoEmptyDirectory()
        {
            var cloneUrl = "https://github.com/foo/bar";
            var clonePath = @"c:\empty\directory";
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Directory.DirectoryExists(clonePath).Returns(true);
            operatingSystem.Directory.IsEmpty(clonePath).Returns(true);
            var vsGitServices = Substitute.For<IVSGitServices>();
            var cloneService = CreateRepositoryCloneService(operatingSystem: operatingSystem, vsGitServices: vsGitServices);
            await cloneService.CloneRepository(cloneUrl, clonePath);

            operatingSystem.Directory.DidNotReceive().CreateDirectory(clonePath);
            await vsGitServices.Received().Clone(cloneUrl, clonePath, true);
        }

        static RepositoryCloneService CreateRepositoryCloneService(IOperatingSystem operatingSystem = null,
            IVSGitServices vsGitServices = null, IUsageTracker usageTracker = null,
            ITeamExplorerServices teamExplorerServices = null, IGitHubServiceProvider serviceProvider = null)
        {
            operatingSystem = operatingSystem ?? Substitute.For<IOperatingSystem>();
            vsGitServices = vsGitServices ?? Substitute.For<IVSGitServices>();
            usageTracker = usageTracker ?? Substitute.For<IUsageTracker>();
            teamExplorerServices = teamExplorerServices ?? Substitute.For<ITeamExplorerServices>();
            serviceProvider = serviceProvider ?? Substitute.For<IGitHubServiceProvider>();

            operatingSystem.Environment.ExpandEnvironmentVariables(Args.String).Returns(x => x[0]);

            return new RepositoryCloneService(operatingSystem, vsGitServices, teamExplorerServices,
                Substitute.For<IGraphQLClientFactory>(), Substitute.For<IGitHubContextService>(),
                usageTracker, serviceProvider, new JoinableTaskContext());
        }
    }
}
