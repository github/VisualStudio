using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnitTests;
using GitHub.Services;
using System.Linq.Expressions;
using System;
using GitHub.Models;

public class RepositoryCloneServiceTests
{
    public class TheCloneRepositoryMethod : TestBaseClass
    {
        [Test]
        public async Task ClonesToRepositoryPathAsync()
        {
            var serviceProvider = Substitutes.GetServiceProvider();
            var operatingSystem = serviceProvider.GetOperatingSystem();
            var vsGitServices = serviceProvider.GetVSGitServices();
            var cloneService = CreateRepositoryCloneService(serviceProvider);

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
            var serviceProvider = Substitutes.GetServiceProvider();
            var usageTracker = serviceProvider.GetUsageTracker();
            var cloneService = CreateRepositoryCloneService(serviceProvider);

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
            var serviceProvider = Substitutes.GetServiceProvider();
            var operatingSystem = serviceProvider.GetOperatingSystem();
            operatingSystem.Directory.DirectoryExists(repositoryPath).Returns(true);
            var dte = Substitute.For<EnvDTE.DTE>();
            serviceProvider.GetService<EnvDTE.DTE>().Returns(dte);
            dte.Solution.FileName.Returns(solutionPath);
            if (isFolder)
            {
                operatingSystem.Directory.DirectoryExists(solutionPath).Returns(true);
            }
            var cloneService = CreateRepositoryCloneService(serviceProvider);

            await cloneService.CloneOrOpenRepository(cloneDialogResult);

            var teamExplorerServices = serviceProvider.GetTeamExplorerServices();
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
            var serviceProvider = Substitutes.GetServiceProvider();
            var operatingSystem = serviceProvider.GetOperatingSystem();
            operatingSystem.Directory.DirectoryExists(repositoryPath).Returns(dirExists);
            var usageTracker = serviceProvider.GetUsageTracker();
            var cloneService = CreateRepositoryCloneService(serviceProvider);

            await cloneService.CloneOrOpenRepository(cloneDialogResult);

            await usageTracker.Received(numberOfCalls).IncrementCounter(
                Arg.Is<Expression<Func<UsageModel.MeasuresModel, int>>>(x =>
                    ((MemberExpression)x.Body).Member.Name == counterName));
        }

        [TestCase(@"c:\default\repo", @"c:\default", 1, nameof(UsageModel.MeasuresModel.NumberOfClonesToDefaultClonePath))]
        [TestCase(@"c:\not_default\repo", @"c:\default", 0, nameof(UsageModel.MeasuresModel.NumberOfClonesToDefaultClonePath))]
        public async Task UpdatesMetricsWhenDefaultClonePath(string targetPath, string defaultPath, int numberOfCalls, string counterName)
        {
            var serviceProvider = Substitutes.GetServiceProvider();
            var vsGitServices = serviceProvider.GetVSGitServices();
            vsGitServices.GetLocalClonePathFromGitProvider().Returns(defaultPath);
            var usageTracker = serviceProvider.GetUsageTracker();
            var cloneService = CreateRepositoryCloneService(serviceProvider);

            await cloneService.CloneRepository("https://github.com/foo/bar", targetPath);
            var model = UsageModel.Create(Guid.NewGuid());

            await usageTracker.Received(numberOfCalls).IncrementCounter(
                Arg.Is<Expression<Func<UsageModel.MeasuresModel, int>>>(x =>
                    ((MemberExpression)x.Body).Member.Name == counterName));
        }

        static RepositoryCloneService CreateRepositoryCloneService(IGitHubServiceProvider sp)
        {
            return new RepositoryCloneService(sp.GetOperatingSystem(), sp.GetVSGitServices(), sp.GetTeamExplorerServices(),
                sp.GetGraphQLClientFactory(), sp.GetGitHubContextService(), sp.GetUsageTracker(), sp);
        }
    }
}
