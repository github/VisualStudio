using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio.Commands;
using NSubstitute;
using NUnit.Framework;

public static class OpenFromUrlCommandTests
{
    public class TheExecuteMethod
    {
        [Test]
        public async Task Executed_From_Menu()
        {
            var target = CreateOpenFromUrlCommand();

            await target.Execute(null);
        }

        [Test]
        public async Task Executed_From_Command_Window()
        {
            var target = CreateOpenFromUrlCommand();

            await target.Execute("");
        }

        [TestCase("https://github.com/github/visualstudio", null, null, 0, 1, Description = "No active repository")]
        [TestCase("https://github.com/github/visualstudio", null, @"c:\source\visualstudio", 0, 1, Description = "Active repository with no remote")]
        [TestCase("https://github.com/github/visualstudio", "https://github.com/github/visualstudio", @"c:\source\visualstudio", 1, 0, Description = "Matching active repository")]
        [TestCase("HTTPS://GITHUB.COM/GITHUB/VISUALSTUDIO", "https://github.com/github/visualstudio", @"c:\source\visualstudio", 1, 0, Description = "Matching active repository with different case")]
        [TestCase("https://github.com/jcansdale/visualstudio", "https://github.com/github/visualstudio", @"c:\source\visualstudio", 0, 1, Description = "Fork of target repository")]
        [TestCase("https://github.com/owner1/repo1", "https://github.com/owner2/repo2", @"c:\source", 0, 1, Description = "Different repository")]
        public async Task Execute(string url, string activeUrl, string activePath, int tryNavigateToContextCalls, int showCloneDialogCalls)
        {
            var dialogService = Substitute.For<IDialogService>();
            var teamExplorerContext = Substitute.For<ITeamExplorerContext>();
            var activeRepository = new LocalRepositoryModel { CloneUrl = activeUrl, LocalPath = activePath };
            teamExplorerContext.ActiveRepository.Returns(activeRepository);
            var gitHubContextService = Substitute.For<IGitHubContextService>();
            gitHubContextService.FindContextFromUrl(url).Returns(new GitHubContext());
            dialogService.ShowCloneDialog(null, url).Returns(new CloneDialogResult(@"c:\source", url));
            var target = CreateOpenFromUrlCommand(dialogService: dialogService,
                teamExplorerContext: teamExplorerContext, gitHubContextService: gitHubContextService);

            await target.Execute(url);

            gitHubContextService.ReceivedWithAnyArgs(tryNavigateToContextCalls).TryNavigateToContext(null, null);
            await dialogService.ReceivedWithAnyArgs(showCloneDialogCalls).ShowCloneDialog(null, null);
        }
    }

    static OpenFromUrlCommand CreateOpenFromUrlCommand(
        IDialogService dialogService = null,
        IRepositoryCloneService repositoryCloneService = null,
        ITeamExplorerContext teamExplorerContext = null,
        IGitHubContextService gitHubContextService = null)
    {
        dialogService = dialogService ?? Substitute.For<IDialogService>();
        repositoryCloneService = repositoryCloneService ?? Substitute.For<IRepositoryCloneService>();
        teamExplorerContext = teamExplorerContext ?? Substitute.For<ITeamExplorerContext>();
        gitHubContextService = gitHubContextService ?? Substitute.For<IGitHubContextService>();

        return new OpenFromUrlCommand(
            new Lazy<IDialogService>(() => dialogService),
            new Lazy<IRepositoryCloneService>(() => repositoryCloneService),
            new Lazy<ITeamExplorerContext>(() => teamExplorerContext),
            new Lazy<IGitHubContextService>(() => gitHubContextService));
    }
}
