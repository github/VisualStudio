using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GitHub;
using GitHub.Cache;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels;
using Moq;
using Xunit;

/// <summary>
/// Tests common to <see cref="MentionsAutoCompleteSource" /> and <see cref="IssuesAutoCompleteSource" />.
/// Run the actual concrete test classes.
/// </summary>
public abstract class AutoCompleteSourceTests<TAutoCompleteSource, TCacheInterface>
    where TAutoCompleteSource : IAutoCompleteSource
    where TCacheInterface : class, IAutoCompleteSourceCache
{
    [Fact]
    public async Task LocalRepositoryDoesNotSupportAutoComplete()
    {
        var container = new TestContainer();
        var localRepository = Mock.Of<IRepositoryModel>();
        container.Setup<ISourceListViewModel>(vm => vm.SelectedRepository).Returns(localRepository);
        var source = container.Get<TAutoCompleteSource>();

        var suggestions = await source.GetSuggestions().ToList();

        Assert.Empty(suggestions);
    }

    [Fact]
    public async Task ReturnsEmptyWhenSourceCacheThrows()
    {
        var container = new TestContainer();
        var gitHubRemote = Mock.Of<IRepositoryHost>(x => x.Address == HostAddress.Create("https://github.com/"));
        var repository = Mock.Of<IRepositoryModel>(x => x.RepositoryHost == gitHubRemote);
        container.Setup<ISourceListViewModel>(vm => vm.SelectedRepository).Returns(repository);
        container.Setup<TCacheInterface>(c => c.RetrieveSuggestions(Args.RepositoryModel))
            .Returns(Observable.Throw<IReadOnlyList<SuggestionItem>>(new Exception("Shit happened!")));
        var source = container.Get<TAutoCompleteSource>();

        var suggestions = await source.GetSuggestions().ToList();

        Assert.Empty(suggestions);
    }

    [Fact]
    public async Task ReturnsResultForGitHubRepository()
    {
        var container = new TestContainer();
        var expectedAvatar = Mock.Of<BitmapSource>();
        var gitHubRepository = CreateRepository("https://github.com");
        container.Setup<ISourceListViewModel>(vm => vm.SelectedRepository).Returns(gitHubRepository);
        container.Setup<IImageCache>(c => c.GetImage(new Uri("https://githubusercontent.com/a/shiftkey.png")))
            .Returns(Observable.Return(expectedAvatar));

        var suggestions = new[]
        {
            new SuggestionItem("shiftkey", "Nice guy", new Uri("https://githubusercontent.com/a/shiftkey.png"))
        };
        container.Setup<TCacheInterface>(c  => c.RetrieveSuggestions(gitHubRepository))
            .Returns(Observable.Return(suggestions));
        var source = container.Get<TAutoCompleteSource>();

        var retrieved = await source.GetSuggestions().ToList();

        Assert.NotEmpty(retrieved);
        Assert.Equal("shiftkey", retrieved[0].Name);
        Assert.Equal("Nice guy", retrieved[0].Description);
        await AssertAvatar(expectedAvatar, retrieved[0]);
    }

    protected IRepositoryModel CreateRepository(string hostAddress)
    {
        var gitHubRemote = Mock.Of<IRepositoryHost>(x => x.Address == HostAddress.Create(hostAddress));
        return Mock.Of<IRepositoryModel>(x => x.RepositoryHost == gitHubRemote);
    }

    protected virtual async Task AssertAvatar(BitmapSource expected, AutoCompleteSuggestion suggestion)
    {
        var avatar = await suggestion.Image;
        Assert.Same(expected, avatar);
    }
}
