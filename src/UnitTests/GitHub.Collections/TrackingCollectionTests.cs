using GitHub.Collections;
using GitHub.Models;
using NSubstitute;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

public class TrackingCollectionTests
{
    public class Sorting : TestBase
    {
#if DEBUG
        public Sorting(ITestOutputHelper output)
        : base(output)
        {
        }
#endif

        [Fact(Skip="")]
        public void SortingAscending()
        {
            var expectedTotal = 20;
            var rnd = new Random();
            var list = Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x => {
                    var s = Substitute.For<IPullRequestModel>();
                    s.Number.Returns(x);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == x);
                    return s;
                })
                .ToObservable();
            var col = new TrackingCollection<IPullRequestModel>(list);
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            var count = 0;
            var evt = new ManualResetEvent(false);
            col.Subscribe(t =>
            {
                if (++count == expectedTotal)
                    evt.Set();
            }, () => { });

            evt.WaitOne();
            evt.Reset();
            Assert.Collection(col, Enumerable.Range(1, expectedTotal).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).ToArray());
        }

        [Fact(Skip = "")]
        public void SortingRandomizedDescending()
        {
            var expectedTotal = 20;
            var rnd = new Random();
            var list = Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x => {
                    var s = Substitute.For<IPullRequestModel>();
                    s.Number.Returns(x);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == x);
                    return s;
                })
                .ToObservable();
            var col = new TrackingCollection<IPullRequestModel>(list);
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.Number).Compare);
            var count = 0;
            var evt = new ManualResetEvent(false);
            col.Subscribe(t =>
            {
                if (++count == expectedTotal)
                    evt.Set();
            }, () => { });

            evt.WaitOne();
            evt.Reset();
            Assert.Collection(col, Enumerable.Range(1, expectedTotal).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).Reverse().ToArray());
        }


        [Fact(Skip = "")]
        public void CollectionIsSortedWhenUpdated()
        {
            var expectedTotal = 20;
            var rnd = new Random();
            var list = Observable.Defer(() =>
                Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x => GetThing(x))
                .ToObservable()
            );
            var list1 = list.Do(s => s.Title = "Cache");
            var list2 = list.Do(s => s.Title = "Live");
            var col = new TrackingCollection<Thing>(list1.Concat(list2));
            col.SetComparer(OrderedComparer<Thing>.OrderBy(x => x.Number).Compare);

            var count = 0;
            var evt = new ManualResetEvent(false);
            col.Subscribe(t =>
            {
                if (++count == expectedTotal * 2)
                    evt.Set();
            }, () => { });

            evt.WaitOne();
            evt.Reset();

            var inspectors = Enumerable.Range(1, expectedTotal).Select(x => new Action<Thing>(t =>
            {
                Assert.Equal(x, t.Number);
                Assert.Equal("Live", t.Title);
            }));
            Assert.Collection(col, inspectors.ToArray());
        }

        [Fact(Skip = "")]
        public void CollectionIsSortedWhenTitleUpdated()
        {
            var expectedTotal = 20;
            var rnd = new Random();
            var list1 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
                .OrderBy(x => rnd.Next(1, expectedTotal))
                .Select(x =>
                {
                    var s = Substitute.For<IPullRequestModel>();
                    s.Title.Returns(((char)('a' + x)).ToString());
                    s.Number.Returns(x);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == x);
                    s.When(c => c.CopyFrom(Arg.Any<IPullRequestModel>())).Do(c =>
                    {
                        var str = c.Arg<IPullRequestModel>().Title;
                        s.Title.Returns(str);
                    });
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();
            var list2 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
                .OrderBy(x => rnd.Next(1, expectedTotal))
                .Select(x =>
                {
                    var s = Substitute.For<IPullRequestModel>();
                    s.Title.Returns(((char)('c' + x)).ToString());
                    s.Number.Returns(x);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == x);
                    s.When(c => c.CopyFrom(Arg.Any<IPullRequestModel>())).Do(c =>
                    {
                        var str = c.Arg<IPullRequestModel>().Title;
                        s.Title.Returns(str);
                    });
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var col = new TrackingCollection<IPullRequestModel>(list1.Concat(list2),
                OrderedComparer<IPullRequestModel>.OrderBy(x => x.Title).Compare);

            var count = 0;
            var evt = new ManualResetEvent(false);
            col.Subscribe(t =>
            {
                if (++count == expectedTotal*2)
                    evt.Set();
            }, () => { });

            evt.WaitOne();
            evt.Reset();
            Assert.NotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());
            Assert.Collection(col, Enumerable.Range(1, expectedTotal).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(((char)('c' + x)).ToString(), t.Title))).ToArray());
        }

        [Fact(Skip = "")]
        public void CollectionIsSortedWhenComparerUpdated()
        {
            var expectedTotal = 20;
            var rnd = new Random();
            var titles1 = Enumerable.Range(1, expectedTotal).Select(x => ((char)('a' + x)).ToString()).ToList();
            var idstack1 = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));
            var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

            var titles2 = Enumerable.Range(1, expectedTotal).Select(x => ((char)('c' + x)).ToString()).ToList();
            var idstack2 = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));
            var titlestack2 = new Stack<string>(titles2.OrderBy(_ => rnd.Next()));

            var list1 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack1.Pop();
                    var title = titlestack1.Pop();
                    return GetThing(id, title);
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var list2 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack2.Pop();
                    var title = titlestack2.Pop();
                    return GetThing(id, title);
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var col = new TrackingCollection<Thing>(list1.Concat(list2),
                OrderedComparer<Thing>.OrderBy(x => x.Title).Compare);

            var count = 0;
            var evt = new ManualResetEvent(false);
            col.Subscribe(t =>
            {
                if (++count == expectedTotal * 2)
                    evt.Set();
            }, () => { });

            evt.WaitOne();
            evt.Reset();

            Assert.NotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());
            Assert.Collection(col, titles2.Select(x => new Action<Thing>(t => Assert.Equal(x, t.Title))).ToArray());

            col.SetComparer(OrderedComparer<Thing>.OrderBy(x => x.Number).Compare);
            Assert.Collection(col, Enumerable.Range(1, expectedTotal).Select(x => new Action<Thing>(t => Assert.Equal(x, t.Number))).ToArray());
        }


        [Fact(Skip = "")]
        public void SortingInPlace()
        {
            var expectedTotal = 20;
            var rnd = new Random();

            var titles1 = Enumerable.Range(1, expectedTotal).Select(x => ((char)('a' + x)).ToString()).ToList();
            var dates1 = Enumerable.Range(1, expectedTotal).Select(x => DateTimeOffset.UtcNow + TimeSpan.FromMinutes(x)).ToList();

            var idstack1 = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));
            var datestack1 = new Stack<DateTimeOffset>(dates1.OrderBy(_ => rnd.Next()));
            var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

            var titles2 = Enumerable.Range(1, expectedTotal).Select(x => ((char)('c' + x)).ToString()).ToList();
            var dates2 = Enumerable.Range(1, expectedTotal).Select(x => DateTimeOffset.UtcNow + TimeSpan.FromMinutes(x)).ToList();

            var idstack2 = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));
            var datestack2 = new Stack<DateTimeOffset>(dates2.OrderBy(_ => rnd.Next()));
            var titlestack2 = new Stack<string>(titles2.OrderBy(_ => rnd.Next()));

            var list1 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack1.Pop();
                    var date = datestack1.Pop();
                    var title = titlestack1.Pop();

                    var s = Substitute.For<IPullRequestModel>();
                    s.Title.Returns(title);
                    s.CreatedAt.Returns(date);
                    s.Number.Returns(id);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == id);
                    s.When(c => c.CopyFrom(Arg.Any<IPullRequestModel>())).Do(c =>
                    {
                        var newtitle = c.Arg<IPullRequestModel>().Title;
                        var newdate = c.Arg<IPullRequestModel>().CreatedAt;
                        s.Title.Returns(newtitle);
                        s.CreatedAt.Returns(newdate);
                    });
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var list2 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack2.Pop();
                    var date = datestack2.Pop();
                    var title = titlestack2.Pop();

                    var s = Substitute.For<IPullRequestModel>();
                    s.Title.Returns(title);
                    s.CreatedAt.Returns(date);
                    s.Number.Returns(id);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == id);
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();


            var col = new TrackingCollection<IPullRequestModel>(list1.Concat(list2),
                OrderedComparer<IPullRequestModel>.OrderBy(x => x.CreatedAt).Compare);

            var count = 0;
            var evt = new ManualResetEvent(false);
            col.Subscribe(t =>
            {
                if (++count == expectedTotal * 2)
                    evt.Set();
            }, () => { });

            evt.WaitOne();
            evt.Reset();

            // it's initially sorted by date, so id list should not match
            Assert.NotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());
            Assert.Collection(col, dates2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.CreatedAt))).ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            Assert.Collection(col, Enumerable.Range(1, expectedTotal).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Title).Compare);
            Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).ToArray());
        }

        [Fact(Skip = "")]
        public void MultipleSortingWithDifferentDirections()
        {
            var expectedTotal = 20;
            var rnd = new Random();

            var titles1 = Enumerable.Range(1, expectedTotal).Select(x => ((char)('a' + x)).ToString()).ToList();
            var dates1 = Enumerable.Range(1, expectedTotal).Select(x => DateTimeOffset.UtcNow + TimeSpan.FromMinutes(x)).ToList();

            var idstack1 = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));
            var datestack1 = new Stack<DateTimeOffset>(dates1.OrderBy(_ => rnd.Next()));
            var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

            var titles2 = Enumerable.Range(1, expectedTotal).Select(x => ((char)('c' + x)).ToString()).ToList();
            var dates2 = Enumerable.Range(1, expectedTotal).Select(x => DateTimeOffset.UtcNow + TimeSpan.FromMinutes(x)).ToList();

            var idstack2 = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));
            var datestack2 = new Stack<DateTimeOffset>(dates2.OrderBy(_ => rnd.Next()));
            var titlestack2 = new Stack<string>(titles2.OrderBy(_ => rnd.Next()));

            var list1 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack1.Pop();
                    var date = datestack1.Pop();
                    var title = titlestack1.Pop();

                    var s = new Thing();
                    s.Title = title;
                    s.CreatedAt = date;
                    s.Number = id;
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var list2 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack2.Pop();
                    var date = datestack2.Pop();
                    var title = titlestack2.Pop();

                    var s = new Thing();
                    s.Title = title;
                    s.CreatedAt = date;
                    s.Number = id;
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var col = new TrackingCollection<Thing>(list1.Concat(list2),
                OrderedComparer<Thing>.OrderByDescending(x => x.CreatedAt).Compare);

            var count = 0;
            var evt = new ManualResetEvent(false);
            col.Subscribe(t =>
            {
                if (++count == expectedTotal * 2)
                    evt.Set();
            }, () => { });

            evt.WaitOne();
            evt.Reset();

            // it's initially sorted by date, so id list should not match
            Assert.NotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());

            Assert.Collection(col, dates2.Select(x => new Action<Thing>(t => Assert.Equal(x, t.CreatedAt))).Reverse().ToArray());

            col.SetComparer(OrderedComparer<Thing>.OrderBy(x => x.Number).Compare);
            Assert.Collection(col, Enumerable.Range(1, expectedTotal).Select(x => new Action<Thing>(t => Assert.Equal(x, t.Number))).ToArray());

            col.SetComparer(OrderedComparer<Thing>.OrderBy(x => x.CreatedAt).Compare);
            Assert.Collection(col, dates2.Select(x => new Action<Thing>(t => Assert.Equal(x, t.CreatedAt))).ToArray());

            col.SetComparer(OrderedComparer<Thing>.OrderByDescending(x => x.Title).Compare);
            Assert.Collection(col, titles2.Select(x => new Action<Thing>(t => Assert.Equal(x, t.Title))).Reverse().ToArray());

            col.SetComparer(OrderedComparer<Thing>.OrderBy(x => x.Title).Compare);
            Assert.Collection(col, titles2.Select(x => new Action<Thing>(t => Assert.Equal(x, t.Title))).ToArray());
        }
    }
}
