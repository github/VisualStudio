using System;
using System.Collections.Generic;
using GitHub.Services;
using NUnit.Framework;
using NSubstitute;

public class NavigationServiceTests
{
    public class TheFindNearestMatchingLineMethod
    {
        [TestCase(new[] { "line" }, new[] { "line" }, 0, 0, Description = "Match same line")]
        [TestCase(new[] { "line" }, new[] { "line_no_match" }, 0, -1, Description = "No matching line")]
        [TestCase(new[] { "line" }, new[] { "", "line" }, 0, 1, Description = "Match line moved up")]
        [TestCase(new[] { "", "line" }, new[] { "line" }, 1, 0, Description = "Match line moved down")]
        [TestCase(new[] { "line", "line" }, new[] { "line", "line" }, 0, 0, Description = "Match nearest line")]
        [TestCase(new[] { "line", "line" }, new[] { "line", "line" }, 1, 1, Description = "Match nearest line")]
        [TestCase(new[] { "line" }, new[] { "line" }, 1, 0, Description = "Treat after last line the same as last line")]
        public void FindNearestMatching(IList<string> fromLines, IList<string> toLines, int line, int expectLine)
        {
            var sp = Substitute.For<IServiceProvider>();
            var target = new NavigationService(sp);

            var matchingLine = target.FindNearestMatchingLine(fromLines, toLines, line);

            Assert.That(matchingLine, Is.EqualTo(expectLine));
        }
    }
}
