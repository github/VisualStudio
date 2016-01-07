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

public class GistCreationViewModelTests
{
    static IGistCreationViewModel CreateViewModel(string selectedText = null )
    {
        var selectedTextProvider = Substitute.For<ISelectedTextProvider>();
        selectedTextProvider.GetSelectedText().Returns(Observable.Return(selectedText));
        var repositoryHost = Substitutes.ServiceProvider.GetRepositoryHosts().GitHubHost;

        return new GistCreationViewModel(repositoryHost, selectedTextProvider);
    }

    public class TheCreateGistCommand : TestBaseClass
    {
        [Theory]
        [InlineData("Console.WriteLine", "Gist.cs", true)]
        [InlineData("Console.WriteLine", "Gist.cs", false)]
        public void CreatesAGistUsingTheApiClient(string selectedText, string fileName, bool isPrivate)
        {
            var selectedTextProvider = Substitute.For<ISelectedTextProvider>();
            selectedTextProvider.GetSelectedText().Returns(Observable.Return(selectedText));
            var repositoryHost = Substitutes.ServiceProvider.GetRepositoryHosts().GitHubHost;

            var vm = new GistCreationViewModel(repositoryHost, selectedTextProvider)
            {
                FileName = fileName,
                IsPrivate = isPrivate
            };

            vm.CreateGist.Execute(null);

            repositoryHost.ApiClient
                .Received()
                .CreateGist(
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
            var vm = CreateViewModel();
            vm.FileName = fileName;

            var actual = vm.CreateGist.CanExecute(null);
            Assert.Equal(expected, actual);
        }
    }
}
