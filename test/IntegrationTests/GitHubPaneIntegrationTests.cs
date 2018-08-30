using GitHub.VisualStudio;
using Xunit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace IntegrationTests
{
    public class GitHubPaneIntegrationTests
    {
        const string GitHubPaneGuid = "{6B0FDC0A-F28E-47A0-8EED-CC296BEFF6D2}";

        [VsFact(UIThread = true, Version = "2015-")]
        public void ShowGitHubPane()
        {
            var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            var window = dte.Windows.Item(GitHubPaneGuid);
            window.Visible = false;
            var command = dte.Commands.Item(Guids.guidGitHubCmdSet, PkgCmdIDList.showGitHubPaneCommand);

            Assert.False(window.Visible);
            Assert.True(command.IsAvailable);

            dte.Commands.Raise(command.Guid, command.ID, null, null);

            Assert.True(window.Visible);
        }
    }
}
