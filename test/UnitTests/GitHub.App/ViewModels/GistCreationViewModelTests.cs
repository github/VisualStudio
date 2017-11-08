using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Octokit;
using UnitTests;
using Xunit;
using System.Reactive.Linq;
using GitHub.Models;
using ReactiveUI;
using GitHub.SampleData;

public class GistCreationViewModelTests
{
    static IGistCreationViewModel CreateViewModel(IServiceProvider provider, string selectedText = "", string fileName = "", bool isPrivate = false)
    {
        var selectedTextProvider = Substitute.For<ISelectedTextProvider>();
        selectedTextProvider.GetSelectedText().Returns(selectedText);
        var repositoryHost = provider.GetRepositoryHosts().GitHubHost;
        var accounts = new ReactiveList<IAccount>() { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
        repositoryHost.ModelService.GetAccounts().Returns(Observable.Return(accounts));
        var gistPublishService = provider.GetGistPublishService();
        return new GistCreationViewModel(repositoryHost, selectedTextProvider, gistPublishService, Substitute.For<IUsageTracker>())
        {
            FileName = fileName,
            IsPrivate = isPrivate
        };
    }

    public class TheCreateGistCommand : TestBaseClass
    {
        [Theory]
        [InlineData("Console.WriteLine", "Gist.cs", true)]
        [InlineData("Console.WriteLine", "Gist.cs", false)]
        public void CreatesAGistUsingTheApiClient(string selectedText, string fileName, bool isPrivate)
        {
            var provider = Substitutes.ServiceProvider;
            var vm = CreateViewModel(provider, selectedText, fileName, isPrivate);
            var gistPublishService = provider.GetGistPublishService();
            var repositoryHost = provider.GetRepositoryHosts().GitHubHost;
            vm.CreateGist.Execute(null);

            gistPublishService
                .Received()
                .PublishGist(
                    repositoryHost.ApiClient,
                    Arg.Is<NewGist>(g => g.Public == !isPrivate
                        && g.Files.First().Key == fileName
                        && g.Files.First().Value == selectedText));
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Gist.cs", true)]
        public void CannotCreateGistIfFileNameIsMissing(string fileName, bool expected)
        {
            var provider = Substitutes.ServiceProvider;
            var vm = CreateViewModel(provider, fileName: fileName);

            var actual = vm.CreateGist.CanExecute(null);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Foo()
        {
            var x = new PullRequestDetailViewModelDesigner();
        }
    }
}
