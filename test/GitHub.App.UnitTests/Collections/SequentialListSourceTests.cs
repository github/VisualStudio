using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;
using NUnit.Framework;

namespace GitHub.App.UnitTests.Collections
{
    public class SequentialListSourceTests
    {
        [Test]
        public async Task GetCount_Should_Load_First_Page()
        {
            var target = new TestSource();

            Assert.That(target.PagesLoaded, Is.Empty);

            var count = await target.GetCount();

            Assert.That(count, Is.EqualTo(100));
            Assert.That(target.PagesLoaded, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public async Task GetPage_Should_Load_Pages()
        {
            var target = new TestSource();

            Assert.That(target.PagesLoaded, Is.Empty);

            var count = await target.GetPage(3);

            Assert.That(target.PagesLoaded, Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void GetPage_Should_Stop_Loading_Pages_When_LoadPage_Throws()
        {
            var target = new TestSource(2);

            Assert.That(target.PagesLoaded, Is.Empty);

            Assert.ThrowsAsync<AggregateException>(() => target.GetPage(3));
            Assert.That(target.PagesLoaded, Is.EqualTo(new[] { 0, 1 }));
        }

        [Test]
        public void IsLoading_Should_Be_Set_To_False_When_LoadPage_Throws()
        {
            var target = new TestSource(2);

            Assert.That(target.PagesLoaded, Is.Empty);

            Assert.ThrowsAsync<AggregateException>(() => target.GetPage(3));
            Assert.That(target.IsLoading, Is.False);
        }

        class TestSource : SequentialListSource<string, string>
        {
            const int PageCount = 10;
            readonly int? throwAtPage;

            public TestSource(int? throwAtPage = null)
            {
                this.throwAtPage = throwAtPage;
            }

            public override int PageSize => 10;
            public List<int> PagesLoaded { get; private set; } = new List<int>();

            protected override string CreateViewModel(string model)
            {
                return model + " loaded";
            }

            protected override Task<Page<string>> LoadPage(string after)
            {
                var page = after != null ? int.Parse(after) : 0;

                if (page == throwAtPage)
                {
                    throw new GitHubLogicException("Thrown.");
                }

                PagesLoaded.Add(page);

                return Task.FromResult(new Page<string>
                {
                    EndCursor = (page + 1).ToString(),
                    HasNextPage = page < PageCount,
                    Items = Enumerable.Range(page * PageSize, PageSize).Select(x => "Item " + x).ToList(),
                    TotalCount = PageSize * PageCount,
                });
            }
        }
    }
}
