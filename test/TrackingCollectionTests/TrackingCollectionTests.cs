#if !DISABLE_REACTIVE_UI
using ReactiveUI;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.Collections;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using NUnit.Framework;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using GitHub;
using System.Globalization;

[TestFixture, Ignore("These tests are flaky and we're planning to move from using TrackingCollection")]
public class TrackingTests : TestBase
{
    const int Timeout = 2000;

    [Test]
    public void OrderByUpdatedNoFilter()
    {
        var count = 6;
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare);
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i, "Run 1")).ToList());
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, i + count, "Run 2")).ToList());

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        count = 0;
        // add first items
        foreach (var l in list1)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(list1.Count, col.Count);
        list1.Sort(new LambdaComparer<Thing>(OrderedComparer<Thing>.OrderByDescending(x => x.CreatedAt).Compare));
        CollectionAssert.AreEqual(col, list1);

        count = 0;
        // replace items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(list2.Count, col.Count);
        CollectionAssert.AreEqual(col, list2);

        col.Dispose();
    }

    [Test]
    public void OrderByUpdatedFilter()
    {
        var count = 3;
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => true,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.ProcessingDelay = TimeSpan.Zero;

        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i, "Run 1")).ToList());
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, i + count, "Run 2")).ToList());

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        count = 0;
        // add first items
        foreach (var l in list1)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(list1.Count, col.Count);
        list1.Sort(new LambdaComparer<Thing>(OrderedComparer<Thing>.OrderByDescending(x => x.CreatedAt).Compare));
        CollectionAssert.AreEqual(col, list1);

        count = 0;
        // replace items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(list2.Count, col.Count);
        CollectionAssert.AreEqual(col, list2);

        col.Dispose();
    }

    [Test]
    public void OnlyIndexes2To4()
    {
        var count = 6;

        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i, "Run 1")).ToList());

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => position >= 2 && position <= 4);
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        count = 0;
        // add first items
        foreach (var l in list1)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(3, col.Count);

#if DEBUG
        CollectionAssert.AreEqual(list1.Reverse<Thing>(), (col as TrackingCollection<Thing>).DebugInternalList);
#endif

        CollectionAssert.AreEqual(col, new List<Thing>() { list1[3], list1[2], list1[1] });

        col.Dispose();
    }

    [Test]
    public void OnlyTimesEqualOrHigherThan3Minutes()
    {
        var count = 6;

        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i, "Run 1")).ToList());

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt >= Now + TimeSpan.FromMinutes(3) && item.UpdatedAt <= Now + TimeSpan.FromMinutes(5));
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        count = 0;
        // add first items
        foreach (var l in list1)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(3, col.Count);

#if DEBUG
        CollectionAssert.AreEqual(list1.Reverse<Thing>(), (col as TrackingCollection<Thing>).DebugInternalList);
#endif
        CollectionAssert.AreEqual(col, new List<Thing>() { list1[2], list1[1], list1[0] });
        col.Dispose();
    }

    [Test]
    public void OrderByDescendingNoFilter()
    {
        var count = 6;

        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i, "Run 1")).ToList());
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, i, "Run 2")).ToList());

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        count = 0;
        // add first items
        foreach (var l in list1)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(6, col.Count);
#if DEBUG
        CollectionAssert.AreEqual(list1, (col as TrackingCollection<Thing>).DebugInternalList);
#endif
        CollectionAssert.AreEqual(col, list1);

        count = 0;
        // add first items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(6, col.Count);

        col.Dispose();
    }

    [Test]
    public void OrderByDescendingNoFilter1000()
    {
        var count = 1000;
        var total = 1000;

        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i, "Run 1")).ToList());
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i, "Run 2")).ToList());

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        count = 0;
        // add first items
        foreach (var l in list1)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();
        Assert.AreEqual(total, col.Count);
        CollectionAssert.AreEqual(col, list1);

        count = 0;
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();
        Assert.AreEqual(total, col.Count);

        count = 0;
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();
        Assert.AreEqual(total, col.Count);

        col.Dispose();
    }


    [Test]
    public void NotInitializedCorrectlyThrows1()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        Assert.Throws<InvalidOperationException>(() => col.Subscribe());
    }

    [Test]
    public void NotInitializedCorrectlyThrows2()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        Assert.Throws<InvalidOperationException>(() => col.Subscribe(_ => { }, () => { }));
    }

    [Test]
    public void NoChangingAfterDisposed1()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(Observable.Never<Thing>(), OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.Dispose();
        Assert.Throws<ObjectDisposedException>(() => col.AddItem(new Thing()));
    }

    [Test]
    public void NoChangingAfterDisposed2()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(Observable.Never<Thing>(), OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.Dispose();
        Assert.Throws<ObjectDisposedException>(() => col.RemoveItem(new Thing()));
    }

    [Test]
    public void FilterTitleRun2()
    {
        var count = 0;
        var total = 1000;

        var list1 = new List<Thing>(Enumerable.Range(1, total).Select(i => GetThing(i, i, i, "Run 1")).ToList());
        var list2 = new List<Thing>(Enumerable.Range(1, total).Select(i => GetThing(i, i, i + 1, "Run 2")).ToList());

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            list1.ToObservable(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => item.Title.Equals("Run 2", StringComparison.Ordinal));
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(total, count);
        Assert.AreEqual(0, col.Count);

        count = 0;

        // add new items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(total, count);
        Assert.AreEqual(total, col.Count);
        CollectionAssert.AreEqual(col, list2.Reverse<Thing>());

        col.Dispose();
    }

    [Test, Category("Timings")]
    public void OrderByDoesntMatchOriginalOrderTimings()
    {
        var count = 0;
        var total = 1000;

        var list1 = new List<Thing>(Enumerable.Range(1, total).Select(i => GetThing(i, i, i, "Run 1")).ToList());
        var list2 = new List<Thing>(Enumerable.Range(1, total).Select(i => GetThing(i, i, i + 1, "Run 2")).ToList());

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            list1.ToObservable(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => item.Title.Equals("Run 2", StringComparison.Ordinal));
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        var start = DateTimeOffset.UtcNow;

        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        evt.WaitOne();
        var time = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
        Assert.LessOrEqual(time, 100);
        evt.Reset();

        Assert.AreEqual(total, count);
        Assert.AreEqual(0, col.Count);

        count = 0;

        start = DateTimeOffset.UtcNow;
        // add new items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        time = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
        Assert.LessOrEqual(time, 200);
        evt.Reset();

        Assert.AreEqual(total, count);
        Assert.AreEqual(total, col.Count);
        CollectionAssert.AreEqual(col, list2.Reverse<Thing>());

        col.Dispose();
    }

    [Test]
    public void OrderByMatchesOriginalOrder()
    {
        var count = 0;
        var total = 1000;

        var list1 = new List<Thing>(Enumerable.Range(1, total).Select(i => GetThing(i, i, i, "Run 1")).Reverse().ToList());
        var list2 = new List<Thing>(Enumerable.Range(1, total).Select(i => GetThing(i, i, i + 1, "Run 2")).Reverse().ToList());

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            list1.ToObservable(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => item.Title.Equals("Run 2", StringComparison.Ordinal));
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        count = 0;
        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(total, count);
        Assert.AreEqual(0, col.Count);

        count = 0;

        // add new items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(total, count);
        Assert.AreEqual(total, col.Count);
        CollectionAssert.AreEqual(col, list2);

        col.Dispose();
    }

    [Test]
    public void SortingTest()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        // testing ADD
        expectedCount = 1;
        // add a thing with UpdatedAt=0:0:10
        Add(source, GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();

        CollectionAssert.AreEqual(col, new List<Thing> { GetThing(1, 10) });

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        CollectionAssert.AreEqual(col, new List<Thing>
        {
            GetThing(1, 10),
            GetThing(2, 2)
        });

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing>
        {
            GetThing(2, 12),
            GetThing(1, 10),
        });

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:12,0:0:11,0:0:10}
        CollectionAssert.AreEqual(col, new List<Thing>
        {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:13,0:0:12,0:0:11,0:0:10,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing>
        {
            GetThing(5, 14),
            GetThing(6, 13),
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(4, 5),
        });

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:13,0:0:12,0:0:11,0:0:10,0:0:5,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing>
        {
            GetThing(6, 13),
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:13,0:0:12,0:0:11,0:0:10,0:0:5,0:0:4}
        CollectionAssert.AreEqual(col, new List<Thing>
        {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
            GetThing(6, 4),
        });

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:13,0:0:12,0:0:11,0:0:10,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing>
        {
            GetThing(6, 14),
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:10,0:0:5,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(3, 14),
            GetThing(2, 12),
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:10,0:0:9,0:0:5,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(3, 14),
            GetThing(1, 10),
            GetThing(2, 9),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:10,0:0:9,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(3, 14),
            GetThing(5, 12),
            GetThing(1, 10),
            GetThing(2, 9),
            GetThing(4, 5),
        });

        expectedCount = 14;
        col.RemoveItem(GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:9,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(3, 14),
            GetThing(5, 12),
            GetThing(2, 9),
            GetThing(4, 5),
        });

        col.Dispose();
    }

    [Test]
    public void SortingTestWithFilterTrue()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => true);
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        // testing ADD
        expectedCount = 1;
        // add a thing with UpdatedAt=0:0:10
        Add(source, GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> { GetThing(1, 10) });

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 10),
            GetThing(2, 2),
        });

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:12,0:0:10}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(1, 10),
        });

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:12,0:0:11,0:0:10}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:13,0:0:12,0:0:11,0:0:10,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(5, 14),
            GetThing(6, 13),
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(4, 5),
        });

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:13,0:0:12,0:0:11,0:0:10,0:0:5,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 13),
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:13,0:0:12,0:0:11,0:0:10,0:0:5,0:0:4}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
            GetThing(6, 4),
        });

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:13,0:0:12,0:0:11,0:0:10,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:10,0:0:5,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(3, 14),
            GetThing(2, 12),
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:10,0:0:9,0:0:5,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(3, 14),
            GetThing(1, 10),
            GetThing(2, 9),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:10,0:0:9,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(3, 14),
            GetThing(5, 12),
            GetThing(1, 10),
            GetThing(2, 9),
            GetThing(4, 5),
        });

        expectedCount = 14;
        col.RemoveItem(GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:9,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 14),
            GetThing(3, 14),
            GetThing(5, 12),
            GetThing(2, 9),
            GetThing(4, 5),
        });

        col.Dispose();
    }

    [Test]
    public void SortingTestWithFilterBetween6And12()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt.Minute >= 6 && item.UpdatedAt.Minute <= 12);
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        // testing ADD
        expectedCount = 1;
        // add a thing with UpdatedAt=0:0:10
        Add(source, GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 10),
        });

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 10),
        });

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(1, 10),
        });

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(1, 10),
        });

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 10),
            GetThing(2, 9),
        });

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(5, 12),
            GetThing(1, 10),
            GetThing(2, 9),
        });

        expectedCount = 14;
        col.RemoveItem(GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(5, 12),
            GetThing(2, 9),
        });

        col.Dispose();
    }


    [Test]
    public void SortingTestWithFilterPosition2to4()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => position >= 2 && position <= 4);
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        // testing ADD
        expectedCount = 1;
        // add a thing with UpdatedAt=0:0:10
        Add(source, GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing>());

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        CollectionAssert.AreEqual(col, new List<Thing>());

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing>());

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 10),
        });

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(5, 5),
        });

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 10),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 11),
            GetThing(1, 10),
            GetThing(5, 5),
        });

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(1, 10),
            GetThing(5, 5),
        });

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 10),
            GetThing(2, 9),
            GetThing(5, 5),
        });

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(5, 12),
            GetThing(1, 10),
            GetThing(2, 9),
        });

        expectedCount = 14;
        col.RemoveItem(GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(5, 12),
            GetThing(2, 9),
            GetThing(4, 5),
        });

        col.Dispose();
    }

    [Test]
    public void SortingTestWithFilterPosition1And3to4()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => position == 1 || (position >= 3 && position <= 4));
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        // testing ADD
        expectedCount = 1;
        // add a thing with UpdatedAt=0:0:10
        Add(source, GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing>());

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
        });

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 10),
        });

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 11),
        });

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(6, 13),
            GetThing(3, 11),
            GetThing(1, 10),
        });

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(1, 10),
            GetThing(5, 5),
        });

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 11),
            GetThing(5, 5),
            GetThing(4, 5),
        });

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 12),
            GetThing(1, 10),
            GetThing(5, 5),
        });

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 14),
            GetThing(1, 10),
            GetThing(5, 5),
        });

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 14),
            GetThing(2, 9),
            GetThing(5, 5),
        });

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 14),
            GetThing(1, 10),
            GetThing(2, 9),
        });

        expectedCount = 14;
        Add(source, GetThing(3, 13));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 13),
            GetThing(1, 10),
            GetThing(2, 9),
        });

        expectedCount = 15;
        col.RemoveItem(GetThing(1, 10));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:9,0:0:5}
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 13),
            GetThing(2, 9),
            GetThing(4, 5),
        });

        col.Dispose();
    }


    [Test]
    public void SortingTestWithFilterMoves()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => position == 1 || position == 2 || position == 5 || position == 6 || position == 7);
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 9;
        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 3));
        Add(source, GetThing(3, 5));
        Add(source, GetThing(4, 7));
        Add(source, GetThing(5, 9));
        Add(source, GetThing(6, 11));
        Add(source, GetThing(7, 13));
        Add(source, GetThing(8, 15));
        Add(source, GetThing(9, 17));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 3),
            GetThing(3, 5),
            GetThing(6, 11),
            GetThing(7, 13),
            GetThing(8, 5),
        });

        expectedCount = 10;
        Add(source, GetThing(7, 4));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(2, 3),
            GetThing(7, 4),
            GetThing(5, 9),
            GetThing(6, 11),
            GetThing(8, 5),
        });

        expectedCount = 11;
        Add(source, GetThing(9, 2));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(9, 2),
            GetThing(2, 3),
            GetThing(4, 7),
            GetThing(5, 9),
            GetThing(6, 11),
        });

        col.Dispose();
    }

    [Test]
    public void ChangingItemContentRemovesItFromFilteredList()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.CreatedAt).Compare,
            (item, position, list) => item.UpdatedAt < now + TimeSpan.FromMinutes(6));
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 5;
        Add(source, GetThing(1, 1, 1));
        Add(source, GetThing(3, 3, 3));
        Add(source, GetThing(5, 5, 5));
        Add(source, GetThing(7, 7, 7));
        Add(source, GetThing(9, 9, 9));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 1, 1),
            GetThing(3, 3, 3),
            GetThing(5, 5, 5),
        });

        expectedCount = 6;
        Add(source, GetThing(5, 5, 6));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 1, 1),
            GetThing(3, 3, 3),
        });
        col.Dispose();
    }

    [Test]
    public void ChangingItemContentRemovesItFromFilteredList2()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.CreatedAt).Compare,
            (item, position, list) => item.UpdatedAt > now + TimeSpan.FromMinutes(2) && item.UpdatedAt < now + TimeSpan.FromMinutes(8));
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 5;
        Add(source, GetThing(1, 1, 1));
        Add(source, GetThing(3, 3, 3));
        Add(source, GetThing(5, 5, 5));
        Add(source, GetThing(7, 7, 7));
        Add(source, GetThing(9, 9, 9));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 3, 3),
            GetThing(5, 5, 5),
            GetThing(7, 7, 7),
        });

        expectedCount = 6;
        Add(source, GetThing(7, 7, 8));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 3, 3),
            GetThing(5, 5, 5),
        });

        expectedCount = 7;
        Add(source, GetThing(7, 7, 7));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 3, 3),
            GetThing(5, 5, 5),
            GetThing(7, 7, 7),
        });

        expectedCount = 8;
        Add(source, GetThing(3, 3, 2));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(5, 5, 5),
            GetThing(7, 7, 7),
        });

        expectedCount = 9;
        Add(source, GetThing(3, 3, 3));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 3, 3),
            GetThing(5, 5, 5),
            GetThing(7, 7, 7),
        });

        expectedCount = 10;
        Add(source, GetThing(5, 5, 1));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 3, 3),
            GetThing(7, 7, 7),
        });
        col.Dispose();
    }

    [Test]
    public void ChangingFilterUpdatesCollection()
    {
        var source = new Subject<Thing>();
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt < Now + TimeSpan.FromMinutes(10));
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 9;
        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 2));
        Add(source, GetThing(3, 3));
        Add(source, GetThing(4, 4));
        Add(source, GetThing(5, 5));
        Add(source, GetThing(6, 6));
        Add(source, GetThing(7, 7));
        Add(source, GetThing(8, 8));
        Add(source, GetThing(9, 9));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 1),
            GetThing(2, 2),
            GetThing(3, 3),
            GetThing(4, 4),
            GetThing(5, 5),
            GetThing(6, 6),
            GetThing(7, 7),
            GetThing(8, 8),
            GetThing(9, 9),
        });

        col.Filter = (item, position, list) => item.UpdatedAt < Now + TimeSpan.FromMinutes(8);

        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(1, 1),
            GetThing(2, 2),
            GetThing(3, 3),
            GetThing(4, 4),
            GetThing(5, 5),
            GetThing(6, 6),
            GetThing(7, 7),
        });
        col.Dispose();
    }

    [Test]
    public void ChangingSortUpdatesCollection()
    {
        var source = new Subject<Thing>();
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt < Now + TimeSpan.FromMinutes(10));
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var evt = new ManualResetEvent(false);
        var list1 = new List<Thing> {
            GetThing(1, 1),
            GetThing(2, 2),
            GetThing(3, 3),
            GetThing(4, 4),
            GetThing(5, 5),
            GetThing(6, 6),
            GetThing(7, 7),
            GetThing(8, 8),
            GetThing(9, 9),
        };

        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });


        foreach (var l in list1)
            Add(source, l);
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, list1);

        col.Comparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;

        CollectionAssert.AreEqual(col, list1.Reverse<Thing>().ToArray());
        col.Dispose();
    }

    [Test]
    public void AddingItemsToCollectionManuallyThrows()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(Observable.Empty<Thing>());
        Assert.Throws<InvalidOperationException>(() => col.Add(GetThing(1)));
        col.Dispose();
    }

    [Test]
    public void InsertingItemsIntoCollectionManuallyThrows()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(Observable.Empty<Thing>());
        Assert.Throws<InvalidOperationException>(() => col.Insert(0, GetThing(1)));
        col.Dispose();
    }

    [Test]
    public void MovingItemsIntoCollectionManuallyThrows()
    {
        var source = new Subject<Thing>();
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(source) { ProcessingDelay = TimeSpan.Zero };
        var count = 0;
        var expectedCount = 2;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        Assert.Throws<InvalidOperationException>(() => (col as TrackingCollection<Thing>).Move(0, 1));
        col.Dispose();
    }

    [Test]
    public void RemovingItemsFromCollectionManuallyThrows()
    {
        var source = new Subject<Thing>();
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(source) { ProcessingDelay = TimeSpan.Zero };
        var count = 0;
        var expectedCount = 2;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        Assert.Throws<InvalidOperationException>(() => col.Remove(GetThing(1)));
        col.Dispose();
    }

    [Test]
    public void RemovingItemsFromCollectionManuallyThrows2()
    {
        var source = new Subject<Thing>();
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(source) { ProcessingDelay = TimeSpan.Zero };
        var count = 0;
        var expectedCount = 2;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        Assert.Throws<InvalidOperationException>(() => col.RemoveAt(0));
        col.Dispose();
    }


    [Test]
    public void ChangingComparers()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(source, OrderedComparer<Thing>.OrderBy(x => x.CreatedAt).Compare);
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var evt = new ManualResetEvent(false);
        var list1 = new List<Thing> {
            GetThing(1, 1, 9),
            GetThing(2, 2, 8),
            GetThing(3, 3, 7),
            GetThing(4, 4, 6),
            GetThing(5, 5, 5),
            GetThing(6, 6, 4),
            GetThing(7, 7, 3),
            GetThing(8, 8, 2),
            GetThing(9, 9, 1),
        };

        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        foreach (var l in list1)
            Add(source, l);

        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(col, list1);
        col.Comparer = null;
        CollectionAssert.AreEqual(col, list1.Reverse<Thing>().ToArray());
        col.Dispose();
    }

    [Test]
    public void Removing()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => (position > 2 && position < 5) || (position > 6 && position < 8));
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 11;
        Add(source, GetThing(0, 0));
        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 2));
        Add(source, GetThing(3, 3));
        Add(source, GetThing(4, 4));
        Add(source, GetThing(5, 5));
        Add(source, GetThing(6, 6));
        Add(source, GetThing(7, 7));
        Add(source, GetThing(8, 8));
        Add(source, GetThing(9, 9));
        Add(source, GetThing(10, 10));

        Assert.True(evt.WaitOne(80));
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(3, 3),
            GetThing(4, 4),
            GetThing(7, 7),
        });

        expectedCount = 12;
        col.RemoveItem(GetThing(2));
        Assert.True(evt.WaitOne(40));
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(4, 4),
            GetThing(5, 5),
            GetThing(8, 8),
        });

        expectedCount = 13;
        col.RemoveItem(GetThing(5));
        Assert.True(evt.WaitOne(40));
        evt.Reset();
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(4, 4),
            GetThing(6, 6),
            GetThing(9, 9),
        });

        col.Filter = null;

        expectedCount = 14;
        col.RemoveItem(GetThing(100)); // this one won't result in a new element from the observable
        col.RemoveItem(GetThing(10));
        Assert.True(evt.WaitOne(40));
        evt.Reset();

        Assert.AreEqual(8, col.Count);
        CollectionAssert.AreEqual(col, new List<Thing> {
            GetThing(0, 0),
            GetThing(1, 1),
            GetThing(3, 3),
            GetThing(4, 4),
            GetThing(6, 6),
            GetThing(7, 7),
            GetThing(8, 8),
            GetThing(9, 9),
        });

        col.Dispose();
    }

    [Test, Category("CodeCoverageFlake")]
    public void RemovingFirstItemWithFilterWorks()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Range(0, 5).Select(x => GetThing(x, x)),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => true);
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 5;
        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        Assert.True(evt.WaitOne(40));
        evt.Reset();

        expectedCount = 6;
        col.RemoveItem(GetThing(0));

        Assert.True(evt.WaitOne(40));
        evt.Reset();

        CollectionAssert.AreEqual(col, Enumerable.Range(1, 4).Select(x => GetThing(x, x)));

        col.Dispose();

    }

    [Test]
    public void DisposingThrows()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(Observable.Empty<Thing>());
        col.Dispose();
        Assert.Throws<ObjectDisposedException>(() => col.Filter = null);
        Assert.Throws<ObjectDisposedException>(() => col.Comparer = null);
        Assert.Throws<ObjectDisposedException>(() => col.Subscribe());
        Assert.Throws<ObjectDisposedException>(() => col.AddItem(GetThing(1)));
        Assert.Throws<ObjectDisposedException>(() => col.RemoveItem(GetThing(1)));
    }

    [Test, Category("Timings")]
    public async Task MultipleSortingAndFiltering()
    {
        var expectedTotal = 20;
        var rnd = new Random(214748364);

        var updatedAtMinutesStack = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));

        var list1 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
            .OrderBy(rnd.Next)
            .Select(x => GetThing(x, x, x, ((char)('a' + x)).ToString(CultureInfo.InvariantCulture)))
            .ToObservable())
            .Replay()
            .RefCount();

        var list2 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
            .OrderBy(rnd.Next)
            .Select(x => GetThing(x, x, updatedAtMinutesStack.Pop(), ((char)('c' + x)).ToString(CultureInfo.InvariantCulture)))
            .ToObservable())
            .Replay()
            .RefCount();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            list1.Concat(list2),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, idx, list) => idx < 5
        );
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.Subscribe();

        await col.OriginalCompleted.Timeout(TimeSpan.FromMilliseconds(Timeout));

        // it's initially sorted by date, so id list should not match
        CollectionAssert.AreNotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());

        var sortlist = col.ToArray();
        Array.Sort(sortlist, new LambdaComparer<Thing>(OrderedComparer<Thing>
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt).Compare));
        CollectionAssert.AreEqual(sortlist.Take(5), col);

        col.Comparer = OrderedComparer<Thing>.OrderBy(x => x.Number).Compare;
        sortlist = col.ToArray();
        Array.Sort(sortlist, new LambdaComparer<Thing>(OrderedComparer<Thing>.OrderBy(x => x.Number).Compare));
        CollectionAssert.AreEqual(sortlist.Take(5), col);

        col.Comparer = OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare;
        sortlist = col.ToArray();
        Array.Sort(sortlist, new LambdaComparer<Thing>(OrderedComparer<Thing>
            .OrderBy(x => x.UpdatedAt)
            .ThenBy(x => x.CreatedAt).Compare));
        CollectionAssert.AreEqual(sortlist.Take(5), col);

        col.Comparer = OrderedComparer<Thing>.OrderByDescending(x => x.Title).Compare;
        sortlist = col.ToArray();
        Array.Sort(sortlist, new LambdaComparer<Thing>(OrderedComparer<Thing>.OrderByDescending(x => x.Title).Compare));
        CollectionAssert.AreEqual(sortlist.Take(5), col);

        col.Comparer = OrderedComparer<Thing>.OrderBy(x => x.Title).Compare;
        sortlist = col.ToArray();
        Array.Sort(sortlist, new LambdaComparer<Thing>(OrderedComparer<Thing>.OrderBy(x => x.Title).Compare));
        CollectionAssert.AreEqual(sortlist.Take(5), col);

        col.Dispose();
    }

    [Test]
    public async Task ListeningTwiceWorks()
    {
        var count = 10;
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>();
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i, "Run 1")).ToList());
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, i + count, "Run 2")).ToList());

#pragma warning disable 4014
        col.Listen(list1.ToObservable());
        col.Subscribe();
        await col.OriginalCompleted.Timeout(TimeSpan.FromMilliseconds(Timeout));

        col.Listen(list2.ToObservable());
        col.Subscribe();
        await col.OriginalCompleted.Timeout(TimeSpan.FromMilliseconds(Timeout));
#pragma warning restore 4014

        CollectionAssert.AreEqual(list2, col);
    }

    [Test]
    public void AddingWithNoObservableSetThrows()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>();
        Assert.Throws<InvalidOperationException>(() => col.AddItem(new Thing()));
    }

    [Test]
    public void RemovingWithNoObservableSetThrows()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>();
        Assert.Throws<InvalidOperationException>(() => col.RemoveItem(new Thing()));
    }

    [Test]
    public async Task AddingBeforeSubscribingWorks()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(Observable.Empty<Thing>());
        ReplaySubject<Thing> done = new ReplaySubject<Thing>();
        col.AddItem(GetThing(1));
        col.AddItem(GetThing(2));
        var count = 0;
        done.OnNext(null);
        col.Subscribe(t =>
        {
            done.OnNext(t);
            if (++count == 2)
                done.OnCompleted();
        }, () => {});

        await done.Timeout(TimeSpan.FromMilliseconds(500));
        Assert.AreEqual(2, col.Count);
    }

    [Test]
    public void DoesNotUpdateThingIfTimeIsOlder()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare);
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            evt.Set();
        }, () => { });

        var createdAndUpdatedTime = DateTimeOffset.Now;
        var olderUpdateTime = createdAndUpdatedTime.Subtract(TimeSpan.FromMinutes(1));

        const string originalTitle = "Original Thing";

        var originalThing = new Thing(1, originalTitle, createdAndUpdatedTime, createdAndUpdatedTime);
        col.AddItem(originalThing);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(originalTitle, col[0].Title);

        const string updatedTitle = "Updated Thing";

        var updatedThing = new Thing(1, updatedTitle, createdAndUpdatedTime, olderUpdateTime);
        col.AddItem(updatedThing);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(originalTitle, col[0].Title);

        col.Dispose();
    }

    [Test]
    public void DoesNotUpdateThingIfTimeIsEqual()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare);
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            evt.Set();
        }, () => { });

        var createdAndUpdatedTime = DateTimeOffset.Now;

        const string originalTitle = "Original Thing";

        var originalThing = new Thing(1, originalTitle, createdAndUpdatedTime, createdAndUpdatedTime);
        col.AddItem(originalThing);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(originalTitle, col[0].Title);

        const string updatedTitle = "Updated Thing";

        var updatedThing = new Thing(1, updatedTitle, createdAndUpdatedTime, createdAndUpdatedTime);
        col.AddItem(updatedThing);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(originalTitle, col[0].Title);

        col.Dispose();
    }

    [Test]
    public void DoesUpdateThingIfTimeIsNewer()
    {
        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare);
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.ProcessingDelay = TimeSpan.Zero;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            evt.Set();
        }, () => { });

        var createdAndUpdatedTime = DateTimeOffset.Now;
        var newerUpdateTime = createdAndUpdatedTime.Add(TimeSpan.FromMinutes(1));

        const string originalTitle = "Original Thing";

        var originalThing = new Thing(1, originalTitle, createdAndUpdatedTime, createdAndUpdatedTime);
        col.AddItem(originalThing);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(originalTitle, col[0].Title);

        const string updatedTitle = "Updated Thing";

        var updatedThing = new Thing(1, updatedTitle, createdAndUpdatedTime, newerUpdateTime);
        col.AddItem(updatedThing);

        evt.WaitOne();
        evt.Reset();

        Assert.AreEqual(updatedTitle, col[0].Title);

        col.Dispose();
    }


    [Test]
    public void ChangingSortingAndUpdatingItemsUpdatesSortCorrectly()
    {
        var source = new Subject<Thing>();

        ITrackingCollection<Thing> col = new TrackingCollection<Thing>(
            source);
        col.Comparer = OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare;
        col.NewerComparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        col.Filter = (item, position, list) =>
            position == 2 || position == 3 || position == 5 || position == 7;
        col.ProcessingDelay = TimeSpan.Zero;

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 9;
        Enumerable.Range(0, expectedCount)
            .Select(x => GetThing(x, x))
            .ForEach(x => Add(source, x));

        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(new List<Thing> {
            GetThing(2, 2),
            GetThing(3, 3),
            GetThing(5, 5),
            GetThing(7, 7),
        }, col);

        expectedCount = 10;
        Add(source, GetThing(3, 3, 2));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(new List<Thing> {
            GetThing(2, 2),
            GetThing(3, 3),
            GetThing(5, 5),
            GetThing(7, 7),
        }, col);

        expectedCount = 11;
        Add(source, GetThing(3, 3, 4));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(new List<Thing> {
            GetThing(2, 2),
            GetThing(3, 3, 4),
            GetThing(5, 5),
            GetThing(7, 7),
        }, col);

        expectedCount = 12;
        Add(source, GetThing(3, 3, 6));
        evt.WaitOne();
        evt.Reset();
        CollectionAssert.AreEqual(new List<Thing> {
            GetThing(2, 2),
            GetThing(4, 4),
            GetThing(3, 3, 6),
            GetThing(7, 7),
        }, col);

        col.Comparer = OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare;
        CollectionAssert.AreEqual(new List<Thing> {
            GetThing(3, 3, 6),
            GetThing(6, 6),
            GetThing(4, 4),
            GetThing(1, 1),
        }, col);

        expectedCount = 13;
        Add(source, GetThing(4, 4));
        evt.WaitOne();
        evt.Reset();

        CollectionAssert.AreEqual(new List<Thing> {
            GetThing(3, 3, 6),
            GetThing(6, 6),
            GetThing(4, 4),
            GetThing(1, 1),
        }, col);

        expectedCount = 14;
        Add(source, GetThing(4, 4, 6));
        evt.WaitOne();
        evt.Reset();

        CollectionAssert.AreEqual(new List<Thing> {
            GetThing(3, 3, 6),
            GetThing(6, 6),
            GetThing(5, 5),
            GetThing(1, 1),
        }, col);

        expectedCount = 15;
        Add(source, GetThing(5, 5, 6));
        evt.WaitOne();
        evt.Reset();

        CollectionAssert.AreEqual(new List<Thing> {
            GetThing(3, 3, 6),
            GetThing(6, 6),
            GetThing(5, 5, 6),
            GetThing(1, 1),
        }, col);

        col.Dispose();
    }
}
