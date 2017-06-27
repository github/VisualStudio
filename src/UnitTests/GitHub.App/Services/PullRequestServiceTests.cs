using System;
using System.IO;
using System.Text;
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
using Xunit;

public class PullRequestServiceTests : TestBaseClass
{
    public class TheExtractDiffFilesMethod
    {
        [Fact]
        public async Task NotCheckedOut_ExtractFilesFromLocalRepo()
        {
            var baseFileContent = "baseFileContent";
            var headFileContent = "headFileContent";
            var fileName = "fileName";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var checkedOut = false;

            var files = await ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent, baseSha, baseFileContent,
                fileName, checkedOut);

            Assert.Equal(baseFileContent, File.ReadAllText(files.Item1));
            Assert.Equal(headFileContent, File.ReadAllText(files.Item2));
        }

        [Fact]
        public async Task MergeBaseAvailable_UseMergeBaseSha()
        {
            var baseFileContent = "baseFileContent";
            var headFileContent = "headFileContent";
            var mergeBaseFileContent = "mergeBaseFileContent";
            var fileName = "fileName";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var mergeBaseSha = "mergeBaseSha";
            var checkedOut = false;

            var files = await ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent, mergeBaseSha, mergeBaseFileContent,
                fileName, checkedOut);

            Assert.Equal(mergeBaseFileContent, File.ReadAllText(files.Item1));
            Assert.Equal(headFileContent, File.ReadAllText(files.Item2));
        }

        [Fact]
        public async void MergeBaseNotAvailable_ThrowsFileNotFoundException()
        {
            var baseFileContent = "baseFileContent";
            var headFileContent = "headFileContent";
            var mergeBaseFileContent = null as string;
            var fileName = "fileName";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var mergeBaseSha = null as string;
            var checkedOut = false;
            var expectMessage = $"Couldn't find merge base between {baseSha} and {headSha}.";

            var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent, mergeBaseSha, mergeBaseFileContent,
                                fileName, checkedOut));
            Assert.Equal(expectMessage, ex.Message);
        }

        [Fact]
        public async Task FileAdded_BaseFileEmpty()
        {
            var baseFileContent = null as string;
            var headFileContent = "headFileContent";
            var fileName = "fileName";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var checkedOut = false;

            var files = await ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent, baseSha, baseFileContent,
                fileName, checkedOut);

            Assert.Equal(string.Empty, File.ReadAllText(files.Item1));
            Assert.Equal(headFileContent, File.ReadAllText(files.Item2));
        }

        [Fact]
        public async Task FileDeleted_HeadFileEmpty()
        {
            var baseFileContent = "baseFileContent";
            var headFileContent = null as string;
            var fileName = "fileName";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var baseRef = new GitReferenceModel("ref", "label", baseSha, "uri");
            var headRef = new GitReferenceModel("ref", "label", headSha, "uri");
            var checkedOut = false;

            var files = await ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent, baseSha, baseFileContent,
                fileName, checkedOut);

            Assert.Equal(baseFileContent, File.ReadAllText(files.Item1));
            Assert.Equal(string.Empty, File.ReadAllText(files.Item2));
        }

        [Fact]
        public async Task CheckedOut_BaseFromWorkingFile()
        {
            var repoDir = "repoDir";
            var baseFileContent = "baseFileContent";
            var headFileContent = null as string;
            var fileName = "fileName";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var baseRef = new GitReferenceModel("ref", "label", baseSha, "uri");
            var checkedOut = true;
            var workingFile = Path.Combine(repoDir, fileName);

            var files = await ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent, baseSha, baseFileContent,
                fileName, checkedOut, repoDir);

            Assert.Equal(baseFileContent, File.ReadAllText(files.Item1));
            Assert.Equal(workingFile, files.Item2);
        }

        // https://github.com/github/VisualStudio/issues/1010
        [Theory]
        [InlineData("utf-8")]        // Unicode (UTF-8)
        [InlineData("Windows-1252")] // Western European (Windows)        
        public async Task CheckedOut_DifferentEncodings(string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            var repoDir = Path.GetTempPath();
            var baseFileContent = "baseFileContent";
            var headFileContent = null as string;
            var fileName = "fileName.txt";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var baseRef = new GitReferenceModel("ref", "label", baseSha, "uri");
            var checkedOut = true;
            var workingFile = Path.Combine(repoDir, fileName);
            var workingFileContent = baseFileContent;
            File.WriteAllText(workingFile, workingFileContent, encoding);

            var files = await ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent,
                baseSha, baseFileContent, fileName, checkedOut, repoDir);

            Assert.Equal(File.ReadAllText(files.Item1), File.ReadAllText(files.Item2));
            Assert.Equal(File.ReadAllBytes(files.Item1), File.ReadAllBytes(files.Item2));
        }

        // Files need to be explicitly marked as UTF-8 otherwise Visual Studio will take them to be Windows-1252.
        // Fixes https://github.com/github/VisualStudio/pull/1004#issuecomment-308358520
        [Fact]
        public async Task NotCheckedOut_FilesHaveUTF8Encoding()
        {
            var baseFileContent = "baseFileContent";
            var headFileContent = "headFileContent";
            var fileName = "fileName";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var checkedOut = false;

            var files = await ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent, baseSha, baseFileContent,
                fileName, checkedOut);

            Assert.True(HasPreamble(files.Item1, Encoding.UTF8));
            Assert.True(HasPreamble(files.Item2, Encoding.UTF8));
        }

        static bool HasPreamble(string file, Encoding encoding)
        {
            using (var stream = File.OpenRead(file))
            {
                foreach (var b in encoding.GetPreamble())
                {
                    if (b != stream.ReadByte())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        [Fact]
        public async Task HeadBranchNotAvailable_ThrowsFileNotFoundException()
        {
            var baseFileContent = "baseFileContent";
            var headFileContent = new FileNotFoundException();
            var fileName = "fileName";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var baseRef = new GitReferenceModel("ref", "label", baseSha, "uri");
            var headRef = new GitReferenceModel("ref", "label", headSha, "uri");
            var checkedOut = false;

            await Assert.ThrowsAsync<FileNotFoundException>(() => ExtractDiffFiles(baseSha, baseFileContent, headSha, headFileContent,
                baseSha, baseFileContent, fileName, checkedOut));
        }

        static async Task<Tuple<string, string>> ExtractDiffFiles(
            string baseSha, object baseFileContent, string headSha, object headFileContent, string mergeBaseSha, object mergeBaseFileContent,
            string fileName, bool checkedOut, string repoDir = "repoDir", int pullNumber = 666, string baseRef = "baseRef")
        {
            var repositoryModel = Substitute.For<ILocalRepositoryModel>();
            repositoryModel.LocalPath.Returns(repoDir);

            var pullRequest = Substitute.For<IPullRequestModel>();
            pullRequest.Number.Returns(pullNumber);

            pullRequest.Base.Returns(new GitReferenceModel(baseRef, "label", baseSha, "uri"));
            pullRequest.Head.Returns(new GitReferenceModel("ref", "label", headSha, "uri"));

            var serviceProvider = Substitutes.ServiceProvider;
            var gitClient = MockGitClient();
            var gitService = serviceProvider.GetGitService();
            var service = new PullRequestService(gitClient, gitService, serviceProvider.GetOperatingSystem(), Substitute.For<IUsageTracker>());

            gitClient.GetPullRequestMergeBase(Arg.Any<IRepository>(), Arg.Any<string>(), baseSha, headSha, baseRef, pullNumber).ReturnsForAnyArgs(Task.FromResult(mergeBaseSha));
            gitClient.ExtractFile(Arg.Any<IRepository>(), mergeBaseSha, fileName).Returns(GetFileTask(mergeBaseFileContent));
            gitClient.ExtractFile(Arg.Any<IRepository>(), baseSha, fileName).Returns(GetFileTask(baseFileContent));
            gitClient.ExtractFile(Arg.Any<IRepository>(), headSha, fileName).Returns(GetFileTask(headFileContent));

            return await service.ExtractDiffFiles(repositoryModel, pullRequest, fileName, checkedOut);
        }

        static IObservable<string> GetFileObservable(object fileOrException)
        {
            if (fileOrException is string)
            {
                return Observable.Return((string)fileOrException);
            }

            if (fileOrException is Exception)
            {
                return Observable.Throw<string>((Exception)fileOrException);
            }

            return Observable.Throw<string>(new FileNotFoundException());
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

    [Fact]
    public void CreatePullRequestAllArgsMandatory()
    {
        var serviceProvider = Substitutes.ServiceProvider;
        var service = new PullRequestService(Substitute.For<IGitClient>(), serviceProvider.GetGitService(), serviceProvider.GetOperatingSystem(), Substitute.For<IUsageTracker>());

        IRepositoryHost host = null;
        ILocalRepositoryModel sourceRepo = null;
        ILocalRepositoryModel targetRepo = null;
        string title = null;
        string body = null;
        IBranch source = null;
        IBranch target = null;

        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        host = serviceProvider.GetRepositoryHosts().GitHubHost;
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        sourceRepo = new LocalRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"), "c:\\path");
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        targetRepo = new LocalRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"), "c:\\path");
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        title = "a title";
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        body = "a body";
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        source = new BranchModel("source", sourceRepo);
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        target = new BranchModel("target", targetRepo);
        var pr = service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body);

        Assert.NotNull(pr);
    }

    public class TheCheckoutMethod
    {
        [Fact]
        public async void ShouldCheckoutExistingBranch()
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

            await service.Checkout(localRepo, pr, "pr/123-foo1");

            gitClient.Received().Checkout(Arg.Any<IRepository>(), "pr/123-foo1").Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "branch.pr/123-foo1.ghfvs-pr", "4").Forget();

            Assert.Equal(2, gitClient.ReceivedCalls().Count());
        }

        [Fact]
        public async void ShouldCheckoutLocalBranch()
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
            pr.Head.Returns(new GitReferenceModel("source", "owner:local", "123", "https://foo.bar/owner/repo"));

            await service.Checkout(localRepo, pr, "local");

            gitClient.Received().Fetch(Arg.Any<IRepository>(), "origin").Forget();
            gitClient.Received().Checkout(Arg.Any<IRepository>(), "local").Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "branch.local.ghfvs-pr", "5").Forget();

            Assert.Equal(4, gitClient.ReceivedCalls().Count());
        }

        [Fact]
        public async void ShouldCheckoutBranchFromFork()
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
            pr.Head.Returns(new GitReferenceModel("source", "owner:local", "123", "https://foo.bar/fork/repo.git"));

            await service.Checkout(localRepo, pr, "pr/5-fork-branch");

            gitClient.Received().SetRemote(Arg.Any<IRepository>(), "fork", new Uri("https://foo.bar/fork/repo.git")).Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "remote.fork.created-by-ghfvs", "true").Forget();
            gitClient.Received().Fetch(Arg.Any<IRepository>(), "fork").Forget();
            gitClient.Received().Fetch(Arg.Any<IRepository>(), "fork", "source:pr/5-fork-branch").Forget();
            gitClient.Received().Checkout(Arg.Any<IRepository>(), "pr/5-fork-branch").Forget();
            gitClient.Received().SetTrackingBranch(Arg.Any<IRepository>(), "pr/5-fork-branch", "refs/remotes/fork/source").Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "branch.pr/5-fork-branch.ghfvs-pr", "5").Forget();
            Assert.Equal(7, gitClient.ReceivedCalls().Count());
        }

        [Fact]
        public async void ShouldUseUniquelyNamedRemoteForFork()
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

            var repo = gitService.GetRepository(localRepo.CloneUrl);
            var remote = Substitute.For<Remote>();
            var remoteCollection = Substitute.For<RemoteCollection>();
            remoteCollection["fork"].Returns(remote);
            repo.Network.Remotes.Returns(remoteCollection);

            var pr = Substitute.For<IPullRequestModel>();
            pr.Number.Returns(5);
            pr.Head.Returns(new GitReferenceModel("source", "owner:local", "123", "https://foo.bar/fork/repo.git"));

            await service.Checkout(localRepo, pr, "pr/5-fork-branch");

            gitClient.Received().SetRemote(Arg.Any<IRepository>(), "fork1", new Uri("https://foo.bar/fork/repo.git")).Forget();
            gitClient.Received().SetConfig(Arg.Any<IRepository>(), "remote.fork1.created-by-ghfvs", "true").Forget();
        }
    }

    public class TheGetDefaultLocalBranchNameMethod
    {
        [Fact]
        public async Task ShouldReturnCorrectDefaultLocalBranchName()
        {
            var service = new PullRequestService(
                MockGitClient(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var result = await service.GetDefaultLocalBranchName(localRepo, 123, "Pull requests can be \"named\" all sorts of thing's (sic)");
            Assert.Equal("pr/123-pull-requests-can-be-named-all-sorts-of-thing-s-sic", result);
        }

        [Fact]
        public async Task ShouldReturnCorrectDefaultLocalBranchNameForPullRequestsWithNonLatinChars()
        {
            var service = new PullRequestService(
                MockGitClient(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var result = await service.GetDefaultLocalBranchName(localRepo, 123, "コードをレビューする準備ができたこと");
            Assert.Equal("pr/123", result);
        }

        [Fact]
        public async Task DefaultLocalBranchNameShouldNotClashWithExistingBranchNames()
        {
            var service = new PullRequestService(
                MockGitClient(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var result = await service.GetDefaultLocalBranchName(localRepo, 123, "foo1");
            Assert.Equal("pr/123-foo1-3", result);
        }
    }

    public class TheGetLocalBranchesMethod
    {
        [Fact]
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

            Assert.Equal("source", result.Name);
        }

        [Fact]
        public async Task ShouldReturnMarkedBranchForPullRequestFromFork()
        {
            var repo = Substitute.For<IRepository>();
            var config = Substitute.For<Configuration>();

            var configEntry1 = Substitute.For<ConfigurationEntry<string>>();
            configEntry1.Key.Returns("branch.pr/1-foo.ghfvs-pr");
            configEntry1.Value.Returns("1");
            var configEntry2 = Substitute.For<ConfigurationEntry<string>>();
            configEntry2.Key.Returns("branch.pr/2-bar.ghfvs-pr");
            configEntry2.Value.Returns("2");

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

            Assert.Equal("pr/1-foo", result.Name);
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
        [Fact]
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

            var repo = gitService.GetRepository(localRepo.CloneUrl);
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
            branch1.Remote.Returns(remote1);
            branch2.Remote.Returns(remote1);
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
}
