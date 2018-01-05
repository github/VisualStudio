using System;
using System.IO;
using System.Linq;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.Properties;
using GitHub.Services;
using GitHub.Models;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class DiffServiceTests
    {
        public class TheParseFragmentMethod
        {
            [Test]
            public void ShouldParsePr960()
            {
                var target = new DiffService(Substitute.For<IGitClient>());
                var result = DiffUtilities.ParseFragment(Resources.pr_960_diff).ToList();

                Assert.AreEqual(4, result.Count);

                Assert.AreEqual(11, result[0].OldLineNumber);
                Assert.AreEqual(11, result[0].NewLineNumber);
                Assert.AreEqual(24, result[0].Lines.Count);

                Assert.AreEqual(61, result[1].OldLineNumber);
                Assert.AreEqual(61, result[1].NewLineNumber);
                Assert.AreEqual(21, result[1].Lines.Count);

                Assert.AreEqual(244, result[2].OldLineNumber);
                Assert.AreEqual(247, result[2].NewLineNumber);
                Assert.AreEqual(15, result[2].Lines.Count);

                Assert.AreEqual(268, result[3].OldLineNumber);
                Assert.AreEqual(264, result[3].NewLineNumber);
                Assert.AreEqual(15, result[3].Lines.Count);

                // -    public class UsageTracker : IUsageTracker
                Assert.AreEqual(17, result[0].Lines[7].OldLineNumber);
                Assert.AreEqual(-1, result[0].Lines[7].NewLineNumber);
                Assert.AreEqual(8, result[0].Lines[7].DiffLineNumber);

                // +    public sealed class UsageTracker : IUsageTracker, IDisposable
                Assert.AreEqual(-1, result[0].Lines[8].OldLineNumber);
                Assert.AreEqual(18, result[0].Lines[8].NewLineNumber);
                Assert.AreEqual(9, result[0].Lines[8].DiffLineNumber);

                //      IConnectionManager connectionManager;
                Assert.AreEqual(26, result[0].Lines[17].OldLineNumber);
                Assert.AreEqual(25, result[0].Lines[17].NewLineNumber);
                Assert.AreEqual(18, result[0].Lines[17].DiffLineNumber);
            }
        }
    }
}
