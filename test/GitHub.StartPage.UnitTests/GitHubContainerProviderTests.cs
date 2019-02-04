using System;
using System.Collections.Generic;
using System.Threading;
using GitHub.Models;
using GitHub.Services;
using GitHub.StartPage;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;
using NSubstitute;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;
using ServiceProgressData = Microsoft.VisualStudio.Shell.ServiceProgressData;

public class GitHubContainerProviderTests
{
    public class TheAcquireCodeContainerAsyncMethod
    {
        [Test]
        public async Task CloneOrOpenRepository_CloneDialogResult_Returned_By_ShowCloneDialog()
        {
            var downloadProgress = Substitute.For<IProgress<ServiceProgressData>>();
            var cancellationToken = CancellationToken.None;
            var dialogService = Substitute.For<IDialogService>();
            var result = new CloneDialogResult(@"x:\repo", "https://github.com/owner/repo");
            dialogService.ShowCloneDialog(null).ReturnsForAnyArgs(result);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var target = CreateGitHubContainerProvider(dialogService: dialogService, cloneService: cloneService);

            await target.AcquireCodeContainerAsync(downloadProgress, cancellationToken);

            await cloneService.Received(1).CloneOrOpenRepository(result, downloadProgress, cancellationToken);
        }

        [Test]
        public async Task Pass_DisplayUrl_To_ShowCloneDialog()
        {
            var displayUrl = "https://github.com/owner/displayUrl";
            var browseOnlineUrl = "https://github.com/owner/browseOnlineUrl";
            var remoteCodeContainer = new RemoteCodeContainer("Name", Guid.NewGuid(), new Uri(displayUrl), new Uri(browseOnlineUrl),
                DateTimeOffset.Now, new Dictionary<string, string>());
            var downloadProgress = Substitute.For<IProgress<ServiceProgressData>>();
            var cancellationToken = CancellationToken.None;
            var dialogService = Substitute.For<IDialogService>();
            var result = new CloneDialogResult(@"x:\repo", "https://github.com/owner/repo");
            dialogService.ShowCloneDialog(null).ReturnsForAnyArgs(result);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var target = CreateGitHubContainerProvider(dialogService: dialogService, cloneService: cloneService);

            await target.AcquireCodeContainerAsync(remoteCodeContainer, downloadProgress, cancellationToken);

            await dialogService.Received(1).ShowCloneDialog(Arg.Any<IConnection>(), displayUrl);
        }

        [Test]
        public async Task Completes_When_Returning_CodeContainer()
        {
            var downloadProgress = Substitute.For<IProgress<ServiceProgressData>>();
            var cancellationToken = CancellationToken.None;
            var dialogService = Substitute.For<IDialogService>();
            var result = new CloneDialogResult(@"x:\repo", "https://github.com/owner/repo");
            dialogService.ShowCloneDialog(null).ReturnsForAnyArgs(result);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var target = CreateGitHubContainerProvider(dialogService: dialogService, cloneService: cloneService);

            var codeContainer = await target.AcquireCodeContainerAsync(downloadProgress, cancellationToken);

            Assert.That(codeContainer, Is.Not.Null);
            downloadProgress.Received(1).Report(
                Arg.Is<ServiceProgressData>(x => x.TotalSteps > 0 && x.CurrentStep == x.TotalSteps));
        }

        [Test]
        public async Task Does_Not_Complete_When_CloneDialog_Canceled()
        {
            var downloadProgress = Substitute.For<IProgress<ServiceProgressData>>();
            var cancellationToken = CancellationToken.None;
            var dialogService = Substitute.For<IDialogService>();
            var result = (CloneDialogResult)null;
            dialogService.ShowCloneDialog(null).ReturnsForAnyArgs(result);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var target = CreateGitHubContainerProvider(dialogService: dialogService, cloneService: cloneService);

            var codeContainer = await target.AcquireCodeContainerAsync(downloadProgress, cancellationToken);

            await cloneService.ReceivedWithAnyArgs(0).CloneOrOpenRepository(null, null, null);
            downloadProgress.ReceivedWithAnyArgs(0).Report(null);
            Assert.That(codeContainer, Is.Null);
        }

        static GitHubContainerProvider CreateGitHubContainerProvider(IDialogService dialogService = null,
            IRepositoryCloneService cloneService = null, IUsageTracker usageTracker = null)
        {
            dialogService = dialogService ?? Substitute.For<IDialogService>();
            cloneService = cloneService ?? Substitute.For<IRepositoryCloneService>();
            usageTracker = usageTracker ?? Substitute.For<IUsageTracker>();

            var sp = Substitute.For<IGitHubServiceProvider>();
            sp.GetService<IDialogService>().Returns(dialogService);
            sp.GetService<IRepositoryCloneService>().Returns(cloneService);
            sp.GetService<IUsageTracker>().Returns(usageTracker);

            var gitHubServiceProvider = new Lazy<IGitHubServiceProvider>(() => sp);
            return new GitHubContainerProvider(gitHubServiceProvider);
        }
    }
}
