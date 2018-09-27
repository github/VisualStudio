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
                var result = DiffUtilities.ParseFragment(Properties.Resources.pr_960_diff).ToList();

                Assert.That(4, Is.EqualTo(result.Count));

                Assert.That(11, Is.EqualTo(result[0].OldLineNumber));
                Assert.That(11, Is.EqualTo(result[0].NewLineNumber));
                Assert.That(24, Is.EqualTo(result[0].Lines.Count));

                Assert.That(61, Is.EqualTo(result[1].OldLineNumber));
                Assert.That(61, Is.EqualTo(result[1].NewLineNumber));
                Assert.That(21, Is.EqualTo(result[1].Lines.Count));

                Assert.That(244, Is.EqualTo(result[2].OldLineNumber));
                Assert.That(247, Is.EqualTo(result[2].NewLineNumber));
                Assert.That(15, Is.EqualTo(result[2].Lines.Count));

                Assert.That(268, Is.EqualTo(result[3].OldLineNumber));
                Assert.That(264, Is.EqualTo(result[3].NewLineNumber));
                Assert.That(15, Is.EqualTo(result[3].Lines.Count));

                // -    public class UsageTracker : IUsageTracker
                Assert.That(17, Is.EqualTo(result[0].Lines[7].OldLineNumber));
                Assert.That(-1, Is.EqualTo(result[0].Lines[7].NewLineNumber));
                Assert.That(8, Is.EqualTo(result[0].Lines[7].DiffLineNumber));

                // +    public sealed class UsageTracker : IUsageTracker, IDisposable
                Assert.That(-1, Is.EqualTo(result[0].Lines[8].OldLineNumber));
                Assert.That(18, Is.EqualTo(result[0].Lines[8].NewLineNumber));
                Assert.That(9, Is.EqualTo(result[0].Lines[8].DiffLineNumber));

                //      IConnectionManager connectionManager;
                Assert.That(26, Is.EqualTo(result[0].Lines[17].OldLineNumber));
                Assert.That(25, Is.EqualTo(result[0].Lines[17].NewLineNumber));
                Assert.That(18, Is.EqualTo(result[0].Lines[17].DiffLineNumber));
            }
        }
    }
}
