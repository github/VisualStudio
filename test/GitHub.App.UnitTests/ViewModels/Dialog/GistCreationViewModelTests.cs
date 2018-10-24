using System;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Factories;
using GitHub.Models;
using GitHub.SampleData;
using GitHub.Services;
using GitHub.ViewModels.Dialog;
using NSubstitute;
using Octokit;
using ReactiveUI;
using UnitTests;
using NUnit.Framework;
using System.Threading.Tasks;

public class GistCreationViewModelTests
{
    static IGistCreationViewModel CreateViewModel(IServiceProvider provider, string selectedText = "", string fileName = "", bool isPrivate = false)
    {
        var selectedTextProvider = Substitute.For<ISelectedTextProvider>();
        selectedTextProvider.GetSelectedText().Returns(selectedText);

        var accounts = new ReactiveList<IAccount>() { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
        var modelService = Substitute.For<IModelService>();
        modelService.GetAccounts().Returns(Observable.Return(accounts));

        var modelServiceFactory = Substitute.For<IModelServiceFactory>();
        modelServiceFactory.CreateAsync(null).ReturnsForAnyArgs(modelService);
        modelServiceFactory.CreateBlocking(null).ReturnsForAnyArgs(modelService);

        var gistPublishService = provider.GetGistPublishService();
        var notificationService = Substitute.For<INotificationService>();

        return new GistCreationViewModel(modelServiceFactory, selectedTextProvider, gistPublishService, notificationService, Substitute.For<IUsageTracker>())
        {
            FileName = fileName,
            IsPrivate = isPrivate
        };
    }

    public class TheCreateGistCommand : TestBaseClass
    {
        [TestCase("Console.WriteLine", "Gist.cs", true)]
        [TestCase("Console.WriteLine", "Gist.cs", false)]
        public void CreatesAGistUsingTheApiClient(string selectedText, string fileName, bool isPrivate)
        {
            var provider = Substitutes.ServiceProvider;
            var vm = CreateViewModel(provider, selectedText, fileName, isPrivate);
            var gistPublishService = provider.GetGistPublishService();
            vm.CreateGist.Execute();

            gistPublishService
                .Received()
                .PublishGist(
                    Arg.Any<IApiClient>(),
                    Arg.Is<NewGist>(g => g.Public == !isPrivate
                        && g.Files.First().Key == fileName
                        && g.Files.First().Value == selectedText));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("Gist.cs", true)]
        public async Task CannotCreateGistIfFileNameIsMissing(string fileName, bool expected)
        {
            var provider = Substitutes.ServiceProvider;
            var vm = CreateViewModel(provider, fileName: fileName);

            var actual = await vm.CreateGist.CanExecute.Take(1);
            Assert.That(expected, Is.EqualTo(actual));
        }

        [Test]
        public void Foo()
        {
            var x = new PullRequestDetailViewModelDesigner();
        }
    }
}
