using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using Rothko;
using UnitTests;
using NUnit.Framework;

public class PullRequestServiceTests : TestBaseClass
{
    public class TheIsWorkingDirectoryCleanMethod
    {
        [Test]
        public async Task NewRepo_True()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.True(isClean);
            }
        }

        [Test]
        public async Task UntrackedFile_True()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var file = Path.Combine(repo.Info.WorkingDirectory, "file.txt");
                File.WriteAllText(file, "contents");

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.True(isClean);
            }
        }


        [Test]
        public async Task CommitFile_True()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var file = Path.Combine(repo.Info.WorkingDirectory, "file.txt");
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, file);
                repo.Commit("foo", Author, Author);

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.True(isClean);
            }
        }

        [Test]
        public async Task AddedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.False(isClean);
            }
        }

        [Test]
        public async Task ModifiedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.WriteAllText(file, "contents2");

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.False(isClean);
            }
        }

        [Test]
        public async Task StagedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.WriteAllText(file, "contents2");
                Commands.Stage(repo, path);

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.False(isClean);
            }
        }

        [Test]
        public async Task MissingFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.Delete(file);

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.False(isClean);
            }
        }

        [Test]
        public async Task RemovedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.Delete(file);
                Commands.Stage(repo, path);

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.False(isClean);
            }
        }

        [Test]
        public async Task RenamedInIndexFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var renamedPath = "renamed.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                var renamedFile = Path.Combine(repo.Info.WorkingDirectory, renamedPath);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.Move(file, renamedFile);
                Commands.Stage(repo, path);
                Commands.Stage(repo, renamedPath);

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.False(isClean);
            }
        }

        [Test]
        public async Task RenamedInWorkingDirFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var renamedPath = "renamed.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                var renamedFile = Path.Combine(repo.Info.WorkingDirectory, renamedPath);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.Move(file, renamedFile);

                // NOTE: `RetrieveStatus(new StatusOptions { DetectRenamesInWorkDir = true })` would need to be used
                // for renamed files to appear as `RenamedInWorkingDir` rather than `Missing` and `Untracked`.
                // This isn't required in the current implementation.
                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.False(isClean);
            }
        }

        [Test] // WorkDirModified
        public async Task ChangedSubmodule_True()
        {
            using (var subRepoDir = new TempDirectory())
            using (var subRepo = CreateRepository(subRepoDir))
            using (var repoDir = new TempDirectory())
            using (var repo = CreateRepository(repoDir))
            {
                RepositoryHelpers.CommitFile(subRepo, "readme.txt", "content", Author);
                RepositoryHelpers.AddSubmodule(repo, "sub_name", "sub/path", subRepo);
                repo.Commit("Add submodule", Author, Author);
                RepositoryHelpers.UpdateSubmodules(repo);
                RepositoryHelpers.CommitFile(subRepo, "readme.txt", "content2", Author);
                RepositoryHelpers.AddSubmodule(repo, "sub_name", "sub/path", subRepo);
                repo.Commit("Update submodule", Author, Author);
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);

                var isClean = await service.IsWorkingDirectoryClean(repositoryModel).FirstAsync();

                Assert.True(isClean);
            }
        }
    }

    public class TheCountSubmodulesToSyncMethod
    {
        [Test] // WorkDirDeleted
        public async Task CommittedSubmodule_True()
        {
            using (var subRepoDir = new TempDirectory())
            using (var subRepo = CreateRepository(subRepoDir))
            using (var repoDir = new TempDirectory())
            using (var repo = CreateRepository(repoDir))
            {
                RepositoryHelpers.CommitFile(subRepo, "readme.txt", "content", Author);
                RepositoryHelpers.AddSubmodule(repo, "sub_name", "sub/path", subRepo);
                repo.Commit($"Add submodule", Author, Author);
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(1, Is.EqualTo(count));
            }
        }

        [Test] // WorkDirUninitialized
        public async Task UninitializedSubmodule_True()
        {
            using (var subRepoDir = new TempDirectory())
            using (var subRepo = CreateRepository(subRepoDir))
            using (var repoDir = new TempDirectory())
            using (var repo = CreateRepository(repoDir))
            {
                RepositoryHelpers.CommitFile(subRepo, "readme.txt", "content", Author);
                var subPath = "sub/path";
                RepositoryHelpers.AddSubmodule(repo, "sub_name", subPath, subRepo);
                repo.Commit($"Add submodule", Author, Author);
                var subDir = Path.Combine(repo.Info.WorkingDirectory, subPath);
                Directory.CreateDirectory(subDir);
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(1, Is.EqualTo(count));
            }
        }

        [Test] // WorkDirModified
        public async Task ChangedSubmodule_True()
        {
            using (var subRepoDir = new TempDirectory())
            using (var subRepo = CreateRepository(subRepoDir))
            using (var repoDir = new TempDirectory())
            using (var repo = CreateRepository(repoDir))
            {
                RepositoryHelpers.CommitFile(subRepo, "readme.txt", "content", Author);
                RepositoryHelpers.AddSubmodule(repo, "sub_name", "sub/path", subRepo);
                repo.Commit("Add submodule", Author, Author);
                RepositoryHelpers.UpdateSubmodules(repo);
                RepositoryHelpers.CommitFile(subRepo, "readme.txt", "content2", Author);
                RepositoryHelpers.AddSubmodule(repo, "sub_name", "sub/path", subRepo);
                repo.Commit("Update submodule", Author, Author);
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(1, Is.EqualTo(count));
            }
        }

        // TODO: Find out when `SubmoduleStatus.WorkDirAdded` is used.

        [Test]
        public async Task UpdatedSubmodule_False()
        {
            using (var subRepoDir = new TempDirectory())
            using (var subRepo = CreateRepository(subRepoDir))
            using (var repoDir = new TempDirectory())
            using (var repo = CreateRepository(repoDir))
            {
                RepositoryHelpers.CommitFile(subRepo, "readme.txt", "content", Author);
                RepositoryHelpers.AddSubmodule(repo, "sub_name", "sub/path", subRepo);
                repo.Commit($"Add submodule", Author, Author);
                RepositoryHelpers.UpdateSubmodules(repo);
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task NewRepo_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task UntrackedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var file = Path.Combine(repo.Info.WorkingDirectory, "file.txt");
                File.WriteAllText(file, "contents");

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task CommitFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var file = Path.Combine(repo.Info.WorkingDirectory, "file.txt");
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, file);
                repo.Commit("foo", Author, Author);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task AddedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task ModifiedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.WriteAllText(file, "contents2");

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task StagedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.WriteAllText(file, "contents2");
                Commands.Stage(repo, path);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task MissingFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.Delete(file);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task RemovedFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.Delete(file);
                Commands.Stage(repo, path);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task RenamedInIndexFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var renamedPath = "renamed.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                var renamedFile = Path.Combine(repo.Info.WorkingDirectory, renamedPath);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.Move(file, renamedFile);
                Commands.Stage(repo, path);
                Commands.Stage(repo, renamedPath);

                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }

        [Test]
        public async Task RenamedInWorkingDirFile_False()
        {
            using (var tempDir = new TempDirectory())
            using (var repo = CreateRepository(tempDir))
            {
                var service = CreatePullRequestService(repo);
                var repositoryModel = CreateLocalRepositoryModel(repo);
                var path = "file.txt";
                var renamedPath = "renamed.txt";
                var file = Path.Combine(repo.Info.WorkingDirectory, path);
                var renamedFile = Path.Combine(repo.Info.WorkingDirectory, renamedPath);
                File.WriteAllText(file, "contents");
                Commands.Stage(repo, path);
                repo.Commit("foo", Author, Author);
                File.Move(file, renamedFile);

                // NOTE: `RetrieveStatus(new StatusOptions { DetectRenamesInWorkDir = true })` would need to be used
                // for renamed files to appear as `RenamedInWorkingDir` rather than `Missing` and `Untracked`.
                // This isn't required in the current implementation.
                var count = await service.CountSubmodulesToSync(repositoryModel).FirstAsync();

                Assert.That(0, Is.EqualTo(count));
            }
        }
    }

    protected static Repository CreateRepository(TempDirectory tempDirectory)
    {
        var repoDir = tempDirectory.Directory.FullName;
        return new Repository(Repository.Init(repoDir));
    }

    static PullRequestService CreatePullRequestService(Repository repo)
    {
        var repoDir = repo.Info.WorkingDirectory;
        var serviceProvider = Substitutes.ServiceProvider;
        var gitService = serviceProvider.GetGitService();
        gitService.GetRepository(repoDir).Returns(repo);
        var service = new PullRequestService(Substitute.For<IGitClient>(), gitService, serviceProvider.GetOperatingSystem(), Substitute.For<IUsageTracker>());
        return service;
    }

    static ILocalRepositoryModel CreateLocalRepositoryModel(Repository repo)
    {
        var repoDir = repo.Info.WorkingDirectory;
        var repositoryModel = Substitute.For<ILocalRepositoryModel>();
        repositoryModel.LocalPath.Returns(repoDir);
        return repositoryModel;
    }

    static Signature Author => new Signature("foo", "foo@bar.com", DateTimeOffset.Now);

    public class TheExtractToTempFileMethod
    {
        [Test]
        public async Task ExtractsExistingFile()
        {
            var gitClient = MockGitClient();
            var target = CreateTarget(gitClient);
            var repository = Substitute.For<ILocalRepositoryModel>();
            var fileContent = "file content";
            var pr = CreatePullRequest();

            gitClient.ExtractFile(Arg.Any<IRepository>(), "123", "filename").Returns(GetFileTask(fileContent));
            var file = await target.ExtractToTempFile(repository, pr, "filename", "123", Encoding.UTF8);

            Assert.That(File.ReadAllText(file), Is.EqualTo(fileContent));
        }

        [Test]
        public async Task CreatesEmptyFileForNonExistentFile()
        {
            var gitClient = MockGitClient();
            var target = CreateTarget(gitClient);
            var repository = Substitute.For<ILocalRepositoryModel>();
            var pr = CreatePullRequest();

            gitClient.ExtractFile(Arg.Any<IRepository>(), "123", "filename").Returns(GetFileTask(null));
            var file = await target.ExtractToTempFile(repository, pr, "filename", "123", Encoding.UTF8);

            Assert.That(File.ReadAllText(file), Is.EqualTo(string.Empty));
        }

        // https://github.com/github/VisualStudio/issues/1010
        [TestCase("utf-8")]        // Unicode (UTF-8)
        [TestCase("Windows-1252")] // Western European (Windows)        
        public async Task CanChangeEncoding(string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            var repoDir = Path.GetTempPath();
            var fileName = "fileName.txt";
            var fileContent = "file content";
            var gitClient = MockGitClient();
            var target = CreateTarget(gitClient);
            var repository = Substitute.For<ILocalRepositoryModel>();
            var pr = CreatePullRequest();

            var expectedPath = Path.Combine(repoDir, fileName);
            var expectedContent = fileContent;
            File.WriteAllText(expectedPath, expectedContent, encoding);

            gitClient.ExtractFile(Arg.Any<IRepository>(), "123", "filename").Returns(GetFileTask(fileContent));
            var file = await target.ExtractToTempFile(repository, pr, "filename", "123", encoding);

            Assert.That(File.ReadAllText(expectedPath), Is.EqualTo(File.ReadAllText(file)));
            Assert.That(File.ReadAllBytes(expectedPath), Is.EqualTo(File.ReadAllBytes(file)));
        }

        static IPullRequestModel CreatePullRequest()
        {
            var result = Substitute.For<IPullRequestModel>();
            return result;
        }
    }

    [Test]
    public void CreatePullRequestAllArgsMandatory()
    {
        var serviceProvider = Substitutes.ServiceProvider;
        var service = new PullRequestService(Substitute.For<IGitClient>(), serviceProvider.GetGitService(), serviceProvider.GetOperatingSystem(), Substitute.For<IUsageTracker>());

        IModelService ms = null;
        ILocalRepositoryModel sourceRepo = null;
        ILocalRepositoryModel targetRepo = null;
        string title = null;
        string body = null;
        IBranch source = null;
        IBranch target = null;

        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(ms, sourceRepo, targetRepo, source, target, title, body));

        ms = Substitute.For<IModelService>();
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(ms, sourceRepo, targetRepo, source, target, title, body));

        sourceRepo = new LocalRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"), "c:\\path");
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(ms, sourceRepo, targetRepo, source, target, title, body));

        targetRepo = new LocalRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"), "c:\\path");
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(ms, sourceRepo, targetRepo, source, target, title, body));

        title = "a title";
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(ms, sourceRepo, targetRepo, source, target, title, body));

        body = "a body";
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(ms, sourceRepo, targetRepo, source, target, title, body));

        source = new BranchModel("source", sourceRepo);
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(ms, sourceRepo, targetRepo, source, target, title, body));

        target = new BranchModel("target", targetRepo);
        var pr = service.CreatePullRequest(ms, sourceRepo, targetRepo, source, target, title, body);

        Assert.NotNull(pr);
    }

    public class TheCheckoutMethod
    {
        [Test]
        public async Task ShouldCheckoutExistingBranch()
        {
            var gitClient = MockGitClient();
            var service = new PullRequestService(
                gitClient,
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var pr = Substitute.For<IPullRequestModel>();
            pr.Number.Returns(4);
            pr.Base.Returns(new GitReferenceModel("master", "owner:master", "123", "https://foo.bar/owner/repo.git"));

            await service.Checkout(localRepo, pr, "pr/123-foo1");

            gitClient.Received().Checkout(Arg.Any<IRepository>(), "pr/123-foo1").Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "branch.pr/123-foo1.ghfvs-pr-owner-number", "owner#4").Forget();

            Assert.That(2, Is.EqualTo(gitClient.ReceivedCalls().Count()));
        }

        [Test]
        public async Task ShouldCheckoutLocalBranch()
        {
            var gitClient = MockGitClient();
            var service = new PullRequestService(
                gitClient,
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            localRepo.CloneUrl.Returns(new UriString("https://foo.bar/owner/repo"));

            var pr = Substitute.For<IPullRequestModel>();
            pr.Number.Returns(5);
            pr.Base.Returns(new GitReferenceModel("master", "owner:master", "123", "https://foo.bar/owner/repo.git"));
            pr.Head.Returns(new GitReferenceModel("prbranch", "owner:prbranch", "123", "https://foo.bar/owner/repo"));

            await service.Checkout(localRepo, pr, "prbranch");

            gitClient.Received().Fetch(Arg.Any<IRepository>(), "origin").Forget();
            gitClient.Received().Checkout(Arg.Any<IRepository>(), "prbranch").Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "branch.prbranch.ghfvs-pr-owner-number", "owner#5").Forget();

            Assert.That(4, Is.EqualTo(gitClient.ReceivedCalls().Count()));
        }

        [Test]
        public async Task ShouldCheckoutBranchFromFork()
        {
            var gitClient = MockGitClient();
            var service = new PullRequestService(
                gitClient,
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            localRepo.CloneUrl.Returns(new UriString("https://foo.bar/owner/repo"));

            var pr = Substitute.For<IPullRequestModel>();
            pr.Number.Returns(5);
            pr.Base.Returns(new GitReferenceModel("master", "owner:master", "123", "https://foo.bar/owner/repo.git"));
            pr.Head.Returns(new GitReferenceModel("prbranch", "fork:prbranch", "123", "https://foo.bar/fork/repo.git"));

            await service.Checkout(localRepo, pr, "pr/5-fork-branch");

            gitClient.Received().SetRemote(Arg.Any<IRepository>(), "fork", new Uri("https://foo.bar/fork/repo.git")).Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "remote.fork.created-by-ghfvs", "true").Forget();
            gitClient.Received().Fetch(Arg.Any<IRepository>(), "fork").Forget();
            gitClient.Received().Fetch(Arg.Any<IRepository>(), "fork", "prbranch:pr/5-fork-branch").Forget();
            gitClient.Received().Checkout(Arg.Any<IRepository>(), "pr/5-fork-branch").Forget();
            gitClient.Received().SetTrackingBranch(Arg.Any<IRepository>(), "pr/5-fork-branch", "refs/remotes/fork/prbranch").Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "branch.pr/5-fork-branch.ghfvs-pr-owner-number", "owner#5").Forget();
            Assert.That(7, Is.EqualTo(gitClient.ReceivedCalls().Count()));
        }

        [Test]
        public async Task ShouldUseUniquelyNamedRemoteForFork()
        {
            var gitClient = MockGitClient();
            var gitService = MockGitService();
            var service = new PullRequestService(
                gitClient,
                gitService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            localRepo.CloneUrl.Returns(new UriString("https://foo.bar/owner/repo"));

            using (var repo = gitService.GetRepository(localRepo.CloneUrl))
            {
                var remote = Substitute.For<Remote>();
                var remoteCollection = Substitute.For<RemoteCollection>();
                remoteCollection["fork"].Returns(remote);
                repo.Network.Remotes.Returns(remoteCollection);

                var pr = Substitute.For<IPullRequestModel>();
                pr.Number.Returns(5);
                pr.Base.Returns(new GitReferenceModel("master", "owner:master", "123", "https://foo.bar/owner/repo.git"));
                pr.Head.Returns(new GitReferenceModel("prbranch", "fork:prbranch", "123", "https://foo.bar/fork/repo.git"));

                await service.Checkout(localRepo, pr, "pr/5-fork-branch");

                gitClient.Received().SetRemote(Arg.Any<IRepository>(), "fork1", new Uri("https://foo.bar/fork/repo.git")).Forget();
                gitClient.Received().SetConfig(Arg.Any<IRepository>(), "remote.fork1.created-by-ghfvs", "true").Forget();
            }
        }
    }

    public class TheGetDefaultLocalBranchNameMethod
    {
        [Test]
        public async Task ShouldReturnCorrectDefaultLocalBranchName()
        {
            var service = new PullRequestService(
                MockGitClient(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var result = await service.GetDefaultLocalBranchName(localRepo, 123, "Pull requests can be \"named\" all sorts of thing's (sic)");
            Assert.That("pr/123-pull-requests-can-be-named-all-sorts-of-thing-s-sic", Is.EqualTo(result));
        }

        [Test]
        public async Task ShouldReturnCorrectDefaultLocalBranchNameForPullRequestsWithNonLatinChars()
        {
            var service = new PullRequestService(
                MockGitClient(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var result = await service.GetDefaultLocalBranchName(localRepo, 123, "コードをレビューする準備ができたこと");
            Assert.That("pr/123", Is.EqualTo(result));
        }

        [Test]
        public async Task DefaultLocalBranchNameShouldNotClashWithExistingBranchNames()
        {
            var service = new PullRequestService(
                MockGitClient(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var result = await service.GetDefaultLocalBranchName(localRepo, 123, "foo1");
            Assert.That("pr/123-foo1-3", Is.EqualTo(result));
        }
    }

    public class TheGetLocalBranchesMethod
    {
        [Test]
        public async Task ShouldReturnPullRequestBranchForPullRequestFromSameRepository()
        {
            var service = new PullRequestService(
                MockGitClient(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            localRepo.CloneUrl.Returns(new UriString("https://github.com/foo/bar"));

            var result = await service.GetLocalBranches(localRepo, CreatePullRequest(fromFork: false));

            Assert.That("source", Is.EqualTo(result.Name));
        }

        [Test]
        public async Task ShouldReturnMarkedBranchForPullRequestFromFork()
        {
            var repo = Substitute.For<IRepository>();
            var config = Substitute.For<Configuration>();

            var configEntry1 = Substitute.For<ConfigurationEntry<string>>();
            configEntry1.Key.Returns("branch.pr/1-foo.ghfvs-pr");
            configEntry1.Value.Returns("foo#1");
            var configEntry2 = Substitute.For<ConfigurationEntry<string>>();
            configEntry2.Key.Returns("branch.pr/2-bar.ghfvs-pr");
            configEntry2.Value.Returns("foo#2");

            config.GetEnumerator().Returns(new List<ConfigurationEntry<string>>
            {
                configEntry1,
                configEntry2,
            }.GetEnumerator());

            repo.Config.Returns(config);

            var service = new PullRequestService(
                MockGitClient(),
                MockGitService(repo),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            localRepo.CloneUrl.Returns(new UriString("https://github.com/foo/bar.git"));

            var result = await service.GetLocalBranches(localRepo, CreatePullRequest(true));

            Assert.That("pr/1-foo", Is.EqualTo(result.Name));
        }

        static IPullRequestModel CreatePullRequest(bool fromFork)
        {
            var author = Substitute.For<IAccount>();

            return new PullRequestModel(1, "PR 1", author, DateTimeOffset.Now)
            {
                State = PullRequestStateEnum.Open,
                Body = string.Empty,
                Head = new GitReferenceModel("source", fromFork ? "fork:baz" : "foo:baz", "sha", fromFork ? "https://github.com/fork/bar.git" : "https://github.com/foo/bar.git"),
                Base = new GitReferenceModel("dest", "foo:bar", "sha", "https://github.com/foo/bar.git"),
            };
        }

        static IGitService MockGitService(IRepository repository = null)
        {
            var result = Substitute.For<IGitService>();
            result.GetRepository(Arg.Any<string>()).Returns(repository ?? Substitute.For<IRepository>());
            return result;
        }
    }

    public class TheRemoteUnusedRemotesMethod
    {
        [Test]
        public async Task ShouldRemoveUnusedRemote()
        {
            var gitClient = MockGitClient();
            var gitService = MockGitService();
            var service = new PullRequestService(
                gitClient,
                gitService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            localRepo.CloneUrl.Returns(new UriString("https://github.com/foo/bar"));

            using (var repo = gitService.GetRepository(localRepo.CloneUrl))
            {
                var remote1 = Substitute.For<Remote>();
                var remote2 = Substitute.For<Remote>();
                var remote3 = Substitute.For<Remote>();
                var remotes = new List<Remote> { remote1, remote2, remote3 };
                var remoteCollection = Substitute.For<RemoteCollection>();
                remote1.Name.Returns("remote1");
                remote2.Name.Returns("remote2");
                remote3.Name.Returns("remote3");
                remoteCollection.GetEnumerator().Returns(_ => remotes.GetEnumerator());
                repo.Network.Remotes.Returns(remoteCollection);

                var branch1 = Substitute.For<LibGit2Sharp.Branch>();
                var branch2 = Substitute.For<LibGit2Sharp.Branch>();
                var branches = new List<LibGit2Sharp.Branch> { branch1, branch2 };
                var branchCollection = Substitute.For<BranchCollection>();
                branch1.RemoteName.Returns("remote1");
                branch2.RemoteName.Returns("remote1");
                branchCollection.GetEnumerator().Returns(_ => branches.GetEnumerator());
                repo.Branches.Returns(branchCollection);

                gitClient.GetConfig<bool>(Arg.Any<IRepository>(), "remote.remote1.created-by-ghfvs").Returns(Task.FromResult(true));
                gitClient.GetConfig<bool>(Arg.Any<IRepository>(), "remote.remote2.created-by-ghfvs").Returns(Task.FromResult(true));

                await service.RemoveUnusedRemotes(localRepo);

                remoteCollection.DidNotReceive().Remove("remote1");
                remoteCollection.Received().Remove("remote2");
                remoteCollection.DidNotReceive().Remove("remote3");
            }
        }
    }

    static PullRequestService CreateTarget(
        IGitClient gitClient = null,
        IGitService gitService = null,
        IOperatingSystem os = null,
        IUsageTracker usageTracker = null)
    {
        gitClient = gitClient ?? Substitute.For<IGitClient>();
        gitService = gitService ?? Substitute.For<IGitService>();
        os = os ?? Substitute.For<IOperatingSystem>();
        usageTracker = usageTracker ?? Substitute.For<IUsageTracker>();

        return new PullRequestService(
            gitClient,
            gitService,
            os,
            usageTracker);
    }

    static BranchCollection MockBranches(params string[] names)
    {
        var result = Substitute.For<BranchCollection>();

        foreach (var name in names)
        {
            var branch = Substitute.For<LibGit2Sharp.Branch>();
            branch.CanonicalName.Returns("refs/heads/" + name);
            result[name].Returns(branch);
        }

        return result;
    }

    static IGitClient MockGitClient()
    {
        var result = Substitute.For<IGitClient>();
        var remote = Substitute.For<Remote>();
        remote.Name.Returns("origin");
        result.GetHttpRemote(Arg.Any<IRepository>(), Arg.Any<string>()).Returns(Task.FromResult(remote));
        return result;
    }

    static IGitService MockGitService()
    {
        var repository = Substitute.For<IRepository>();
        var branches = MockBranches("pr/123-foo1", "pr/123-foo1-2");
        repository.Branches.Returns(branches);

        var result = Substitute.For<IGitService>();
        result.GetRepository(Arg.Any<string>()).Returns(repository);
        return result;
    }

    static Task<string> GetFileTask(object content)
    {
        if (content is string)
        {
            return Task.FromResult((string)content);
        }

        if (content is Exception)
        {
            return Task.FromException<string>((Exception)content);
        }

        if (content == null)
        {
            return Task.FromResult<string>(null);
        }

        throw new ArgumentException("Unsupported content type: " + content);
    }
}
