using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using Xunit;

public class GitClientTests
{
    public class ThePushMethod
    {
        [Fact]
        public async Task PushesToDefaultOrigin()
        {
            var origin = Substitute.For<Remote>();
            var head = Substitute.For<Branch>();
            head.Commits.Returns(new FakeCommitLog { Substitute.For<Commit>() });
            var repository = Substitute.For<IRepository>();
            repository.Head.Returns(head);
            repository.Network.Remotes["origin"].Returns(origin);
            var gitClient = new GitClient();

            await gitClient.Push(repository, "master", "origin");

            repository.Network.Received().Push(origin,"HEAD", @"refs/heads/master", null, null, null);
        }

        [Fact]
        public async Task DoesNotPushEmptyRepository()
        {
            var repository = Substitute.For<IRepository>();

            var gitClient = new GitClient();
            await gitClient.Push(repository, "master", "origin");

            repository.Network.DidNotReceive()
                .Push(Args.Remote, Args.String, Args.String, Arg.Any<PushOptions>(), Arg.Any<Signature>(), Args.String);
        }
    }
}
