using System;
using System.Threading.Tasks;
using GitHub.Services;
using GitHub.VisualStudio.Commands;
using Microsoft.VisualStudio;
using NSubstitute;
using NUnit.Framework;

public class OpenFromClipboardCommandTests
{
    public class TheExecuteMethod
    {
        [Test]
        public async Task NothingInClipboard()
        {
            var vsServices = Substitute.For<IVSServices>();
            vsServices.ShowMessageBoxInfo(null).Returns(VSConstants.MessageBoxResult.IDOK);
            var target = CreateOpenFromClipboardCommand(vsServices: vsServices, contextFromClipboard: null);

            await target.Execute(null);

            vsServices.Received(1).ShowMessageBoxInfo(OpenFromClipboardCommand.NoGitHubUrlMessage);
        }

        [Test]
        public async Task NoLocalRepository()
        {
            var context = new GitHubContext();
            var repositoryDir = null as string;
            var vsServices = Substitute.For<IVSServices>();
            var target = CreateOpenFromClipboardCommand(vsServices: vsServices, contextFromClipboard: context, repositoryDir: repositoryDir);

            await target.Execute(null);

            vsServices.Received(1).ShowMessageBoxInfo(OpenFromClipboardCommand.NoActiveRepositoryMessage);
        }

        [Test]
        public async Task DifferentLocalRepository()
        {
            var targetRepositoryName = "targetRepositoryName";
            var activeRepositoryName = "activeRepositoryName";
            var activeRepositoryDir = "activeRepositoryDir";
            var context = new GitHubContext { RepositoryName = targetRepositoryName };
            (string, string, string)? resolveBlobResult = null;
            var vsServices = Substitute.For<IVSServices>();
            var target = CreateOpenFromClipboardCommand(vsServices: vsServices,
                contextFromClipboard: context, repositoryDir: activeRepositoryDir, repositoryName: activeRepositoryName, resolveBlobResult: resolveBlobResult);

            await target.Execute(null);

            vsServices.Received(1).ShowMessageBoxInfo(string.Format(OpenFromClipboardCommand.DifferentRepositoryMessage, context.RepositoryName));
        }

        [TestCase("TargetOwner", "CurrentOwner", OpenFromClipboardCommand.NoResolveDifferentOwnerMessage)]
        [TestCase("SameOwner", "SameOwner", OpenFromClipboardCommand.NoResolveSameOwnerMessage)]
        [TestCase("sameowner", "SAMEOWNER", OpenFromClipboardCommand.NoResolveSameOwnerMessage)]
        public async Task CouldNotResolve(string targetOwner, string currentOwner, string expectMessage)
        {
            var context = new GitHubContext { Owner = targetOwner };
            var repositoryDir = "repositoryDir";
            (string, string, string)? resolveBlobResult = null;
            var vsServices = Substitute.For<IVSServices>();
            var target = CreateOpenFromClipboardCommand(vsServices: vsServices,
                contextFromClipboard: context, repositoryDir: repositoryDir, repositoryOwner: currentOwner, resolveBlobResult: resolveBlobResult);

            await target.Execute(null);

            vsServices.Received(1).ShowMessageBoxInfo(expectMessage);
        }

        [Test]
        public async Task CouldResolve()
        {
            var context = new GitHubContext();
            var repositoryDir = "repositoryDir";
            var resolveBlobResult = ("master", "foo.cs", "");
            var vsServices = Substitute.For<IVSServices>();
            var target = CreateOpenFromClipboardCommand(vsServices: vsServices,
                contextFromClipboard: context, repositoryDir: repositoryDir, resolveBlobResult: resolveBlobResult);

            await target.Execute(null);

            vsServices.DidNotReceiveWithAnyArgs().ShowMessageBoxInfo(null);
        }

        [Test]
        public async Task NoChangesInWorkingDirectory()
        {
            var gitHubContextService = Substitute.For<IGitHubContextService>();
            var context = new GitHubContext();
            var repositoryDir = "repositoryDir";
            var resolveBlobResult = ("master", "foo.cs", "");
            var vsServices = Substitute.For<IVSServices>();
            var target = CreateOpenFromClipboardCommand(gitHubContextService: gitHubContextService, vsServices: vsServices,
                contextFromClipboard: context, repositoryDir: repositoryDir, resolveBlobResult: resolveBlobResult, hasChanges: false);

            await target.Execute(null);

            vsServices.DidNotReceiveWithAnyArgs().ShowMessageBoxInfo(null);
            gitHubContextService.Received(1).TryOpenFile(repositoryDir, context);
        }

        [TestCase(true, null, 1, 0)]
        [TestCase(false, OpenFromClipboardCommand.ChangesInWorkingDirectoryMessage, 1, 1)]
        public async Task HasChangesInWorkingDirectory(bool annotateFileSupported, string message,
            int receivedTryAnnotateFile, int receivedTryOpenFile)
        {
            var gitHubContextService = Substitute.For<IGitHubContextService>();
            gitHubContextService.TryAnnotateFile(null, null, null).ReturnsForAnyArgs(annotateFileSupported);
            var context = new GitHubContext();
            var repositoryDir = "repositoryDir";
            var currentBranch = "currentBranch";
            var resolveBlobResult = ("master", "foo.cs", "");
            var vsServices = Substitute.For<IVSServices>();
            var target = CreateOpenFromClipboardCommand(gitHubContextService: gitHubContextService, vsServices: vsServices,
                contextFromClipboard: context, repositoryDir: repositoryDir, currentBranch: currentBranch, resolveBlobResult: resolveBlobResult, hasChanges: true);

            await target.Execute(null);

            if (message != null)
            {
                vsServices.Received(1).ShowMessageBoxInfo(message);
            }
            else
            {
                vsServices.DidNotReceiveWithAnyArgs().ShowMessageBoxInfo(null);
            }

            await gitHubContextService.Received(receivedTryAnnotateFile).TryAnnotateFile(repositoryDir, currentBranch, context);
            gitHubContextService.Received(receivedTryOpenFile).TryOpenFile(repositoryDir, context);
        }

        static OpenFromClipboardCommand CreateOpenFromClipboardCommand(
            IGitHubContextService gitHubContextService = null,
            ITeamExplorerContext teamExplorerContext = null,
            IVSServices vsServices = null,
            GitHubContext contextFromClipboard = null,
            string repositoryDir = null,
            string repositoryName = null,
            string repositoryOwner = null,
            string currentBranch = null,
            (string, string, string)? resolveBlobResult = null,
            bool? hasChanges = null)
        {
            var sp = Substitute.For<IServiceProvider>();
            gitHubContextService = gitHubContextService ?? Substitute.For<IGitHubContextService>();
            teamExplorerContext = teamExplorerContext ?? Substitute.For<ITeamExplorerContext>();
            vsServices = vsServices ?? Substitute.For<IVSServices>();

            gitHubContextService.FindContextFromClipboard().Returns(contextFromClipboard);
            teamExplorerContext.ActiveRepository.LocalPath.Returns(repositoryDir);
            teamExplorerContext.ActiveRepository.Name.Returns(repositoryName);
            teamExplorerContext.ActiveRepository.Owner.Returns(repositoryOwner);
            teamExplorerContext.ActiveRepository.CurrentBranch.Name.Returns(currentBranch);
            if (resolveBlobResult != null)
            {
                gitHubContextService.ResolveBlob(repositoryDir, contextFromClipboard).Returns(resolveBlobResult.Value);
            }

            if (hasChanges != null)
            {
                gitHubContextService.HasChangesInWorkingDirectory(repositoryDir, resolveBlobResult.Value.Item1, resolveBlobResult.Value.Item2).Returns(hasChanges.Value);
            }

            return new OpenFromClipboardCommand(
                new Lazy<IGitHubContextService>(() => gitHubContextService),
                new Lazy<ITeamExplorerContext>(() => teamExplorerContext),
                new Lazy<IVSServices>(() => vsServices));
        }
    }
}
