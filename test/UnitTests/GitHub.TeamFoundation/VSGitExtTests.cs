using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.VisualStudio.Base;
using NUnit.Framework;
using NSubstitute;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System.Threading.Tasks;
using System.Linq;

public class VSGitExtTests
{
    public class TheConstructor : TestBaseClass
    {
        [TestCase(true, 1)]
        [TestCase(false, 0)]
        public void GetServiceIGitExt_WhenSccProviderContextIsActive(bool isActive, int expectCalls)
        {
            var context = CreateVSUIContext(isActive);
            var sp = Substitute.For<IGitHubServiceProvider>();

            var target = CreateVSGitExt(context, sp: sp);

            sp.Received(expectCalls).GetServiceAsync<IGitExt>();
        }

        [TestCase(true, 1)]
        [TestCase(false, 0)]
        public void GetServiceIGitExt_WhenUIContextChanged(bool activated, int expectCalls)
        {
            var context = CreateVSUIContext(false);
            var sp = Substitute.For<IGitHubServiceProvider>();
            var target = CreateVSGitExt(context, sp: sp);

            var eventArgs = new VSUIContextChangedEventArgs(activated);
            context.UIContextChanged += Raise.Event<EventHandler<VSUIContextChangedEventArgs>>(context, eventArgs);

            sp.Received(expectCalls).GetServiceAsync<IGitExt>();
        }

        [Test]
        public void ActiveRepositories_ReadUsingThreadPoolThread()
        {
            var gitExt = Substitute.For<IGitExt>();
            bool threadPool = false;
            gitExt.ActiveRepositories.Returns(x =>
            {
                threadPool = Thread.CurrentThread.IsThreadPoolThread;
                return new IGitRepositoryInfo[0];
            });

            var target = CreateVSGitExt(gitExt: gitExt);

            Assert.That(threadPool, Is.True);
        }
    }

    public class TheActiveRepositoriesChangedEvent : TestBaseClass
    {
        [Test]
        public async Task GitExtPropertyChangedEvent_ActiveRepositoriesChangedIsFired()
        {
            var context = CreateVSUIContext(true);
            var gitExt = CreateGitExt();

            var target = CreateVSGitExt(context, gitExt);

            bool wasFired = false;
            target.ActiveRepositoriesChanged += () => wasFired = true;
            var eventArgs = new PropertyChangedEventArgs(nameof(gitExt.ActiveRepositories));
            gitExt.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(gitExt, eventArgs);

            await target.PendingTasks;
            Assert.That(wasFired, Is.True);
        }

        [Test]
        public void ExceptionReadingActiveRepositories_StillEmptySoNoEvent()
        {
            var context = CreateVSUIContext(true);
            var gitExt = CreateGitExt(new[] { "repoPath" });
            gitExt.ActiveRepositories.Returns(x => { throw new Exception("Boom!"); });

            var target = CreateVSGitExt(context, gitExt);

            bool wasFired = false;
            target.ActiveRepositoriesChanged += () => wasFired = true;
            var eventArgs = new PropertyChangedEventArgs(nameof(gitExt.ActiveRepositories));
            gitExt.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(gitExt, eventArgs);

            Assert.That(target.ActiveRepositories, Is.Empty);
            Assert.That(wasFired, Is.False);
        }

        [Test]
        public void WhenUIContextChanged_ActiveRepositoriesChangedIsFired()
        {
            var context = CreateVSUIContext(false);
            var gitExt = CreateGitExt();
            var target = CreateVSGitExt(context, gitExt);

            bool wasFired = false;
            target.ActiveRepositoriesChanged += () => wasFired = true;

            var eventArgs = new VSUIContextChangedEventArgs(true);
            context.UIContextChanged += Raise.Event<EventHandler<VSUIContextChangedEventArgs>>(context, eventArgs);
            target.PendingTasks.Wait();

            Assert.That(wasFired, Is.True);
        }

        [Test]
        public void WhenUIContextChanged_FiredUsingThreadPoolThread()
        {
            var context = CreateVSUIContext(false);
            var gitExt = CreateGitExt();
            var target = CreateVSGitExt(context, gitExt);

            bool threadPool = false;
            target.ActiveRepositoriesChanged += () => threadPool = Thread.CurrentThread.IsThreadPoolThread;

            var eventArgs = new VSUIContextChangedEventArgs(true);
            context.UIContextChanged += Raise.Event<EventHandler<VSUIContextChangedEventArgs>>(context, eventArgs);
            target.PendingTasks.Wait();

            Assert.That(threadPool, Is.True);
        }
    }

    public class TheActiveRepositoriesProperty : TestBaseClass
    {
        [Test]
        public void SccProviderContextNotActive_IsEmpty()
        {
            var context = CreateVSUIContext(false);
            var target = CreateVSGitExt(context);

            Assert.That(target.ActiveRepositories, Is.Empty);
        }

        [Test]
        public void SccProviderContextIsActive_InitializeWithActiveRepositories()
        {
            var repoPath = "repoPath";
            var repoFactory = Substitute.For<ILocalRepositoryModelFactory>();
            var context = CreateVSUIContext(true);
            var gitExt = CreateGitExt(new[] { repoPath });
            var target = CreateVSGitExt(context, gitExt, repoFactory: repoFactory);
            target.PendingTasks.Wait();

            var activeRepositories = target.ActiveRepositories;

            Assert.That(activeRepositories.Count, Is.EqualTo(1));
            repoFactory.Received(1).Create(repoPath);
        }

        [Test]
        public void ExceptionRefreshingRepositories_ReturnsEmptyList()
        {
            var repoPath = "repoPath";
            var repoFactory = Substitute.For<ILocalRepositoryModelFactory>();
            repoFactory.Create(repoPath).ReturnsForAnyArgs(x => { throw new Exception("Boom!"); });
            var context = CreateVSUIContext(true);
            var gitExt = CreateGitExt(new[] { repoPath });
            var target = CreateVSGitExt(context, gitExt, repoFactory: repoFactory);
            target.PendingTasks.Wait();

            var activeRepositories = target.ActiveRepositories;

            repoFactory.Received(1).Create(repoPath);
            Assert.That(activeRepositories.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ActiveRepositoriesChangedOrderingShouldBeCorrectAcrossThreads()
        {
            var gitExt = new MockGitExt();
            var repoFactory = new MockRepositoryFactory();
            var target = CreateVSGitExt(gitExt: gitExt, repoFactory: repoFactory);
            var activeRepositories1 = CreateActiveRepositories("repo1");
            var activeRepositories2 = CreateActiveRepositories("repo2");
            var task1 = Task.Run(() => gitExt.ActiveRepositories = activeRepositories1);
            await Task.Delay(1);
            var task2 = Task.Run(() => gitExt.ActiveRepositories = activeRepositories2);

            await Task.WhenAll(task1, task2);
            await target.PendingTasks;

            Assert.That(target.ActiveRepositories.Single().LocalPath, Is.EqualTo("repo2"));
        }
    }

    static IReadOnlyList<IGitRepositoryInfo> CreateActiveRepositories(params string[] repositoryPaths)
    {
        var repositories = new List<IGitRepositoryInfo>();
        foreach (var repositoryPath in repositoryPaths)
        {
            var repoInfo = Substitute.For<IGitRepositoryInfo>();
            repoInfo.RepositoryPath.Returns(repositoryPath);
            repositories.Add(repoInfo);
        }

        return repositories.AsReadOnly();
    }

    static VSGitExt CreateVSGitExt(IVSUIContext context = null, IGitExt gitExt = null, IGitHubServiceProvider sp = null,
        ILocalRepositoryModelFactory repoFactory = null)
    {
        context = context ?? CreateVSUIContext(true);
        gitExt = gitExt ?? CreateGitExt();
        sp = sp ?? Substitute.For<IGitHubServiceProvider>();
        repoFactory = repoFactory ?? Substitute.For<ILocalRepositoryModelFactory>();
        var factory = Substitute.For<IVSUIContextFactory>();
        var contextGuid = new Guid(Guids.GitSccProviderId);
        factory.GetUIContext(contextGuid).Returns(context);
        sp.GetService<IVSUIContextFactory>().Returns(factory);
        sp.GetServiceAsync<IGitExt>().Returns(gitExt);
        var vsGitExt = new VSGitExt(sp, factory, repoFactory);
        vsGitExt.PendingTasks.Wait();
        return vsGitExt;
    }

    static IGitExt CreateGitExt(params string[] repositoryPaths)
    {
        var gitExt = Substitute.For<IGitExt>();
        var repoList = CreateActiveRepositories(repositoryPaths);
        gitExt.ActiveRepositories.Returns(repoList);
        return gitExt;
    }

    static IVSUIContext CreateVSUIContext(bool isActive)
    {
        var context = Substitute.For<IVSUIContext>();
        context.IsActive.Returns(isActive);
        return context;
    }

    class MockGitExt : IGitExt
    {
        IReadOnlyList<IGitRepositoryInfo> activeRepositories = new IGitRepositoryInfo[0];

        public IReadOnlyList<IGitRepositoryInfo> ActiveRepositories
        {
            get { return activeRepositories; }
            set
            {
                if (activeRepositories != value)
                {
                    activeRepositories = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveRepositories)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    class MockRepositoryFactory : ILocalRepositoryModelFactory
    {
        public ILocalRepositoryModel Create(string localPath)
        {
            var result = Substitute.For<ILocalRepositoryModel>();
            result.LocalPath.Returns(localPath);

            if (localPath == "repo1")
            {
                // Trying to force #1493 here by introducing a a delay on the first
                // ActiveRepositories changed notification so that the second completes
                // first.
                Thread.Sleep(10);
            }

            return result;
        }
    }
}
