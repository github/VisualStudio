using System;
using GitHub.InlineReviews.Services;
using GitHub.Services;
using NSubstitute;
using UnitTests.Properties;
using Xunit;

namespace UnitTests.GitHub.InlineReviews
{
    public class InlineCommentBuilderTests
    {
        [Fact]
        public void CorrectlyMapsCommentPositions()
        {
            var target = new InlineCommentBuilder(Substitute.For<IGitClient>());
            var diff = Resources.pr_573_apiclient_diff;
            var result = target.MapDiffPositions(diff, new[] { 31 });

            Assert.Equal(87, result[31]);
        }
    }
}
