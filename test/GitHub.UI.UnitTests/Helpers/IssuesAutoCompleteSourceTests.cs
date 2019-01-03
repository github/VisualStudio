using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GitHub.Cache;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels;
using Xunit;

/// <summary>
/// Tests of the <see cref="IssuesAutoCompleteSource"/>. Test implementations are in
/// <see cref="AutoCompleteSourceTests{TAutoCompleteSource, TCacheInterface}"/>
/// </summary>
/// <remarks>
/// THIS CLASS SHOULD ONLY CONTAIN TESTS SPECIFIC TO <see cref="IssuesAutoCompleteSource"/> that deviate from the
/// behavior in common with all <see cref="IAutoCompleteSource"/> implementations.
/// </remarks>
public class IssuesAutoCompleteSourceTests : AutoCompleteSourceTests<IssuesAutoCompleteSource, IIssuesCache>
{
    [Fact]
    public async Task ReturnsIssuesSortedByLastModified()
    {
        var container = new TestContainer();
        var gitHubRepository = CreateRepository("https://github.com");
        container.Setup<ISourceListViewModel>(vm => vm.SelectedRepository).Returns(gitHubRepository);

        var suggestions = new[]
        {
            new SuggestionItem("100", "We should do this") { LastModifiedDate = DateTimeOffset.UtcNow.AddDays(-1) },
            new SuggestionItem("101", "This shit is broken") { LastModifiedDate = DateTimeOffset.UtcNow },
            new SuggestionItem("102", "What even?")  { LastModifiedDate = DateTimeOffset.UtcNow.AddHours(-1) }
        };
        container.Setup<IIssuesCache>(c => c.RetrieveSuggestions(gitHubRepository))
            .Returns(Observable.Return(suggestions));
        var source = container.Get<IssuesAutoCompleteSource>();

        var retrieved = await source.GetSuggestions().ToList();

        Assert.NotEmpty(retrieved);
        retrieved = retrieved.OrderByDescending(r => r.GetSortRank("")).ToList();
        Assert.Equal("101", retrieved[0].Name);
        Assert.Equal("102", retrieved[1].Name);
        Assert.Equal("100", retrieved[2].Name);
    }

    [Fact]
    public async Task ReturnsIssuesFilteredByIssueNumber()
    {
        var container = new TestContainer();
        var gitHubRepository = CreateRepository("https://github.com");
        container.Setup<ISourceListViewModel>(vm => vm.SelectedRepository).Returns(gitHubRepository);

        var suggestions = new[]
        {
            new SuggestionItem("#200", "We should do this") { LastModifiedDate = DateTimeOffset.UtcNow.AddDays(-1) },
            new SuggestionItem("#101", "This shit is broken") { LastModifiedDate = DateTimeOffset.UtcNow },
            new SuggestionItem("#210", "What even?")  { LastModifiedDate = DateTimeOffset.UtcNow.AddHours(-1) }
        };
        container.Setup<IIssuesCache>(c => c.RetrieveSuggestions(gitHubRepository))
            .Returns(Observable.Return(suggestions));
        var source = container.Get<IssuesAutoCompleteSource>();

        var retrieved = await source.GetSuggestions().ToList();

        Assert.NotEmpty(retrieved);
        retrieved = retrieved.OrderByDescending(r => r.GetSortRank("2")).ToList();
        Assert.Equal("#200", retrieved[0].Name);
        Assert.Equal("#210", retrieved[1].Name);
        Assert.Equal("#101", retrieved[2].Name);
    }

    [Fact]
    public async Task ReturnsIssuesFilteredByText()
    {
        var container = new TestContainer();
        var gitHubRepository = CreateRepository("https://github.com");
        container.Setup<ISourceListViewModel>(vm => vm.SelectedRepository).Returns(gitHubRepository);

        var suggestions = new[]
        {
            new SuggestionItem("#200", "We should do this") { LastModifiedDate = DateTimeOffset.UtcNow.AddDays(-1) },
            new SuggestionItem("#101", "This shit is broken") { LastModifiedDate = DateTimeOffset.UtcNow },
            new SuggestionItem("#210", "What even?")  { LastModifiedDate = DateTimeOffset.UtcNow.AddHours(-1) }
        };
        container.Setup<IIssuesCache>(c => c.RetrieveSuggestions(gitHubRepository))
            .Returns(Observable.Return(suggestions));
        var source = container.Get<IssuesAutoCompleteSource>();

        var retrieved = await source.GetSuggestions().ToList();

        Assert.NotEmpty(retrieved);
        retrieved = retrieved.OrderByDescending(r => r.GetSortRank("shit")).ToList();
        Assert.Equal("#101", retrieved[0].Name); 
        Assert.Equal("#200", retrieved[1].Name);
        Assert.Equal("#210", retrieved[2].Name);
    }

    protected override Task AssertAvatar(BitmapSource expected, AutoCompleteSuggestion suggestion)
    {
        // Issues do not have images associated with them so we'll just ignore the avatar when asserting it.
        return Task.FromResult(0);
    }
}
