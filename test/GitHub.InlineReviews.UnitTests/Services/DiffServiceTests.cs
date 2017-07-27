using System;
using System.IO;
using System.Linq;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.Properties;
using GitHub.Services;
using GitHub.Models;
using NSubstitute;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class DiffServiceTests
    {
        public class TheParseFragmentMethod
        {
            [Fact]
            public void ShouldParsePr960()
            {
                var target = new DiffService(Substitute.For<IGitClient>());
                var result = DiffUtilities.ParseFragment(Resources.pr_960_diff).ToList();

                Assert.Equal(4, result.Count);

                Assert.Equal(11, result[0].OldLineNumber);
                Assert.Equal(11, result[0].NewLineNumber);
                Assert.Equal(24, result[0].Lines.Count);

                Assert.Equal(61, result[1].OldLineNumber);
                Assert.Equal(61, result[1].NewLineNumber);
                Assert.Equal(21, result[1].Lines.Count);

                Assert.Equal(244, result[2].OldLineNumber);
                Assert.Equal(247, result[2].NewLineNumber);
                Assert.Equal(15, result[2].Lines.Count);

                Assert.Equal(268, result[3].OldLineNumber);
                Assert.Equal(264, result[3].NewLineNumber);
                Assert.Equal(15, result[3].Lines.Count);

                // -    public class UsageTracker : IUsageTracker
                Assert.Equal(17, result[0].Lines[7].OldLineNumber);
                Assert.Equal(-1, result[0].Lines[7].NewLineNumber);
                Assert.Equal(12, result[0].Lines[7].DiffLineNumber);

                // +    public sealed class UsageTracker : IUsageTracker, IDisposable
                Assert.Equal(-1, result[0].Lines[8].OldLineNumber);
                Assert.Equal(18, result[0].Lines[8].NewLineNumber);
                Assert.Equal(13, result[0].Lines[8].DiffLineNumber);

                //      IConnectionManager connectionManager;
                Assert.Equal(26, result[0].Lines[17].OldLineNumber);
                Assert.Equal(25, result[0].Lines[17].NewLineNumber);
                Assert.Equal(22, result[0].Lines[17].DiffLineNumber);
            }
        }
    }
}
