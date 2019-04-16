using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.Models;
using NUnit.Framework;

namespace GitHub.UI.UnitTests.Controls
{
    public class AutoCompleteSuggestionTests
    {
        public class TheToStringMethod
        {
            [TestCase(":", ":", ":foo:")]
            [TestCase("@", "", "@foo")]
            [TestCase("#", "", "#foo")]
            [TestCase("@", null, "@foo")]
            public void ReturnsWordSurroundedByPrefixAndSuffix(string prefix, string suffix, string expected)
            {
                var obs = Observable.Return(new BitmapImage());
                var suggestion = new AutoCompleteSuggestion("foo", obs, prefix, suffix);
                Assert.AreEqual(expected, suggestion.ToString());
            }
        }

        public class TheGetSortRankMethod
        {
            [TestCase("pat", "full name", 1)]
            [TestCase("yosemite", "pat name", 0)]
            [TestCase("minnie", "full pat", 0)]
            [TestCase("patrick", "full name", 1)]
            [TestCase("groot", "patrick name", 0)]
            [TestCase("driver", "danica patrick", 0)]
            [TestCase("patricka", "pat name", 1)]
            [TestCase("nomatch", "full name", -1)]
            public void ReturnsCorrectScoreForSuggestions(string login, string name, int expectedRank)
            {
                var obs = Observable.Return(new BitmapImage());

                var suggestion = new AutoCompleteSuggestion(login, name, obs, "@", "");

                int rank = suggestion.GetSortRank("pat");

                Assert.AreEqual(expectedRank, rank);
            }

            [Test]
            public void ReturnsOneForEmptyString()
            {
                var obs = Observable.Return(new BitmapImage());

                var suggestion = new AutoCompleteSuggestion("joe", "namathe", obs, "@", "");

                int rank = suggestion.GetSortRank("");

                Assert.AreEqual(1, rank);
            }
        }
    }
}
