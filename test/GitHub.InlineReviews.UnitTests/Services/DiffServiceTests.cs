using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using GitHub.Services;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class DiffServiceTests
    {
        public class TheDiffMethod
        {
            [Test]
            public async Task DiffReturnsEmptyList()
            {
                var gitClient = Substitute.For<IGitClient>();
                gitClient.Compare(null, null, null, null).ReturnsNull();
                var target = new DiffService(gitClient);

                var result = await target.Diff(null, null, null, null);

                Assert.That(result, Is.Empty);
            }
        }

        public class TheDiffOverloadMethod
        {
            [Test]
            public async Task DiffReturnsEmptyList()
            {
                var gitClient = Substitute.For<IGitClient>();
                gitClient.CompareWith(null, null, null, null, null).ReturnsNull();
                var target = new DiffService(gitClient);

                var result = await target.Diff(null, null, null, null, null);

                Assert.That(result, Is.Empty);
            }
        }
    }
}
