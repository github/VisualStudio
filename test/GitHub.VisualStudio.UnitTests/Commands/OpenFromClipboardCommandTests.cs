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
        public async Task Execute()
        {
            var gitHubContextService = Substitute.For<IGitHubContextService>();
            gitHubContextService.FindContextFromClipboard().Returns(null as GitHubContext);
            var vsServices = Substitute.For<IVSServices>();
            vsServices.ShowMessageBoxInfo(null).Returns(VSConstants.MessageBoxResult.IDOK);
            var target = CreateOpenFromClipboardCommand(gitHubContextService, vsServices: vsServices);

            await target.Execute(null);

            vsServices.Received(1).ShowMessageBoxInfo(OpenFromClipboardCommand.NoGitHubUrlMessage);
        }

        static OpenFromClipboardCommand CreateOpenFromClipboardCommand(
            IGitHubContextService gitHubContextService = null,
            ITeamExplorerContext teamExplorerContext = null,
            IVSServices vsServices = null)
        {
            var sp = Substitute.For<IServiceProvider>();
            gitHubContextService = gitHubContextService ?? Substitute.For<IGitHubContextService>();
            teamExplorerContext = teamExplorerContext ?? Substitute.For<ITeamExplorerContext>();
            vsServices = vsServices ?? Substitute.For<IVSServices>();

            return new OpenFromClipboardCommand(
                new Lazy<IGitHubContextService>(() => gitHubContextService),
                new Lazy<ITeamExplorerContext>(() => teamExplorerContext),
                new Lazy<IVSServices>(() => vsServices));
        }
    }
}
