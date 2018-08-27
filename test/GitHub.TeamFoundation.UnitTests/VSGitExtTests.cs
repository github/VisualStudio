using System;
using System.Linq;
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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;
using static Microsoft.VisualStudio.VSConstants;

public class VSGitExtTests
{
    public class TheConstructor
    {
        [TestCase(true, 1)]
        [TestCase(false, 0)]
        public void GetServiceIGitExt_WhenRepositoryOpenIsActive(bool isActive, int expectCalls)
        {
            var context = CreateVSUIContext(isActive);
            var sp = Substitute.For<IAsyncServiceProvider>();

            var target = CreateVSGitExt(context, sp: sp);

            sp.Received(expectCalls).GetServiceAsync(typeof(IGitExt));
        }

        [TestCase(true, 1)]
        [TestCase(false, 0)]
        public void GetServiceIGitExt_WhenUIContextChanged(bool activated, int expectCalls)
        {
            var context = CreateVSUIContext(false);
            var sp = Substitute.For<IAsyncServiceProvider>();
            var target = CreateVSGitExt(context, sp: sp);

            context.IsActive = activated;
            target.JoinTillEmpty();

            sp.Received(expectCalls).GetServiceAsync(typeof(IGitExt));
        }

        [Test]
        public void ActiveRepositories_ReadUsingThreadPoolThread()
        {
            var gitExt = Substitute.For<IGitExt>();
            bool? threadPool = null;
            gitExt.ActiveRepositories.Returns(x =>
            {
                threadPool = Thread.CurrentThread.IsThreadPoolThread;
                return new IGitRepositoryInfo[0];
            });

            var target = CreateVSGitExt(gitExt: gitExt);
            target.JoinTillEmpty();

            Assert.That(threadPool, Is.True);
        }
    }

    public class TheActiveRepositoriesChangedEvent
    {
        [Test]
        public void GitExtPropertyChangedEvent_ActiveRepositoriesChangedIsFired()
        {
            var context = CreateVSUIContext(true);
            var gitExt = CreateGitExt();

            var target = CreateVSGitExt(context, gitExt);

            bool wasFired = false;
            target.ActiveRepositoriesChanged += () => wasFired = true;
            var eventArgs = new PropertyChangedEventArgs(nameof(gitExt.ActiveRepositories));
            gitExt.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(gitExt, eventArgs);
            target.JoinTillEmpty();

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

            context.IsActive = true;
            target.JoinTillEmpty();

            Assert.That(wasFired, Is.True);
        }

        [Test]
        public void WhenUIContextChanged_FiredUsingThreadPoolThread()
        {
            var context = CreateVSUIContext(false);
            var gitExt = CreateGitExt();
            var target = CreateVSGitExt(context, gitExt);

            bool? threadPool = null;
            target.ActiveRepositoriesChanged += () => threadPool = Thread.CurrentThread.IsThreadPoolThread;

            context.IsActive = true;
            target.JoinTillEmpty();

            Assert.That(threadPool, Is.True);
        }
    }

    public class TheActiveRepositoriesProperty
    {
        [Test]
        public void RepositoryOpenContextNotActive_IsEmpty()
        {
            var context = CreateVSUIContext(false);
            var target = CreateVSGitExt(context);

            Assert.That(target.ActiveRepositories, Is.Empty);
        }

        [Test]
        public void RepositoryOpenIsActive_InitializeWithActiveRepositories()
        {
            var repoPath = "repoPath";
            var repoFactory = Substitute.For<ILocalRepositoryModelFactory>();
            var context = CreateVSUIContext(true);
            var gitExt = CreateGitExt(new[] { repoPath });
            var target = CreateVSGitExt(context, gitExt, repoFactory: repoFactory);
            target.JoinTillEmpty();

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
            target.JoinTillEmpty();

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
            target.JoinTillEmpty();

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

    static VSGitExt CreateVSGitExt(IVSUIContext context = null, IGitExt gitExt = null, IAsyncServiceProvider sp = null,
        ILocalRepositoryModelFactory repoFactory = null, JoinableTaskContext joinableTaskContext = null)
    {
        context = context ?? CreateVSUIContext(true);
        gitExt = gitExt ?? CreateGitExt();
        sp = sp ?? Substitute.For<IAsyncServiceProvider>();
        repoFactory = repoFactory ?? Substitute.For<ILocalRepositoryModelFactory>();
        joinableTaskContext = joinableTaskContext ?? new JoinableTaskContext();
        var factory = Substitute.For<IVSUIContextFactory>();
        factory.GetUIContext(UICONTEXT.RepositoryOpen_guid).Returns(context);
        sp.GetServiceAsync(typeof(IGitExt)).Returns(gitExt);
        var vsGitExt = new VSGitExt(sp, factory, repoFactory, joinableTaskContext);
        vsGitExt.JoinTillEmpty();
        return vsGitExt;
    }

    static IGitExt CreateGitExt(params string[] repositoryPaths)
    {
        var gitExt = Substitute.For<IGitExt>();
        var repoList = CreateActiveRepositories(repositoryPaths);
        gitExt.ActiveRepositories.Returns(repoList);
        return gitExt;
    }

    static MockVSUIContext CreateVSUIContext(bool isActive)
    {
        return new MockVSUIContext { IsActive = isActive };
    }

    class MockVSUIContext : IVSUIContext
    {
        bool isActive;
        Action action;

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                if (isActive && action != null)
                {
                    action.Invoke();
                    action = null;
                }
            }
        }

        public void WhenActivated(Action action)
        {
            if (isActive)
            {
                action.Invoke();
                return;
            }

            this.action = action;
        }
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
