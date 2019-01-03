using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.UI;
using NUnit.Framework;
using Xunit;

public class AutoCompleteSuggestionTests
{
    public class TheToStringMethod
    {
        [Theory]
        [InlineData(":", ":", ":foo:")]
        [InlineData("@", "", "@foo")]
        [InlineData("#", "", "#foo")]
        [InlineData("@", null, "@foo")]
        public void ReturnsWordSurroundedByPrefixAndSuffix(string prefix, string suffix, string expected)
        {
            var obs = Observable.Return(new BitmapImage());
            var suggestion = new AutoCompleteSuggestion("foo", obs, prefix, suffix);
            Assert.Equal(expected, suggestion.ToString());
        }
    }

    public class TheGetSortRankMethod
    {
        [Theory]
        [InlineData("pat", "full name", 1)]
        [InlineData("yosemite", "pat name", 0)]
        [InlineData("minnie", "full pat", 0)]
        [InlineData("patrick", "full name", 1)]
        [InlineData("groot", "patrick name", 0)]
        [InlineData("driver", "danica patrick", 0)]
        [InlineData("patricka", "pat name", 1)]
        [InlineData("nomatch", "full name", -1)]
        public void ReturnsCorrectScoreForSuggestions(string login, string name, int expectedRank)
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestion = new AutoCompleteSuggestion(login, name, obs, "@", "");

            int rank = suggestion.GetSortRank("pat");

            Assert.Equal(expectedRank, rank);
        }

        [Fact]
        public void ReturnsOneForEmptyString()
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestion = new AutoCompleteSuggestion("joe", "namathe", obs, "@", "");

            int rank = suggestion.GetSortRank("");

            Assert.Equal(1, rank);
        }
    }
}
