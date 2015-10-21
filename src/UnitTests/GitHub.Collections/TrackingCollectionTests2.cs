#if !DISABLE_REACTIVE_UI
using ReactiveUI;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.Collections;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;
using Xunit.Abstractions;
using System.Threading;

public class TrackingTests : TestBase
{
#if DEBUG
    public TrackingTests(ITestOutputHelper output)
        : base(output)
    {
    }
#endif

    [Fact]
    public void OrderByUpdatedNoFilter()
    {
        var count = 6;
        var col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare)
        {
            ProcessingDelay = TimeSpan.Zero
        };

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(count - i) })).ToList();
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 2", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(i + count) })).ToList();

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

        Assert.Equal(list1.Count, col.Count);

        var j = 0;
        Assert.Collection(col, list1.Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(count - j, t.Number);
            Assert.Equal(t.UpdatedAt, col[j].UpdatedAt);
            j++;
        })).ToArray());

        count = 0;
        // replace items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.Equal(list2.Count, col.Count);

        j = 0;
        Assert.Collection(col, list2.Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(j + 1, t.Number);
            Assert.Equal(t.UpdatedAt, col[j].UpdatedAt);
            j++;
        })).ToArray());

        col.Dispose();
    }

    [Fact]
    public void OrderByUpdatedFilter()
    {
        var count = 3;
        var col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => true)
        {
            ProcessingDelay = TimeSpan.Zero
        };

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(count - i) })).ToList();
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 2", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(i + count) })).ToList();

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

        Assert.Equal(list1.Count, col.Count);

        var j = 0;
        Assert.Collection(col, list1.Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(count - j, t.Number);
            Assert.Equal(t.UpdatedAt, col[j].UpdatedAt);
            j++;
        })).ToArray());

        count = 0;
        // replace items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.Equal(list2.Count, col.Count);
        j = 0;
        Assert.Collection(col, list2.Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(j + 1, t.Number);
            Assert.Equal(t.UpdatedAt, col[j].UpdatedAt);
            j++;
        })).ToArray());

        col.Dispose();
    }

    [Fact]
    public void OnlyIndexes2To4()
    {
        var count = 6;

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(count - i) })).ToList();

        var col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => position >= 2 && position <= 4)
        {
            ProcessingDelay = TimeSpan.Zero
        };

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

        Assert.Equal(3, col.Count);

        var txtSourceList = @"Source list
0 - id:1 title:Run 1 created:0001-01-01 00:01:00Z updated:0001-01-01 00:05:00Z
1 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
2 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
3 - id:4 title:Run 1 created:0001-01-01 00:04:00Z updated:0001-01-01 00:02:00Z
4 - id:5 title:Run 1 created:0001-01-01 00:05:00Z updated:0001-01-01 00:01:00Z
5 - id:6 title:Run 1 created:0001-01-01 00:06:00Z updated:0001-01-01 00:00:00Z
";
        var txtInternalList = @"Sorted internal list
0 - id:6 title:Run 1 created:0001-01-01 00:06:00Z updated:0001-01-01 00:00:00Z
1 - id:5 title:Run 1 created:0001-01-01 00:05:00Z updated:0001-01-01 00:01:00Z
2 - id:4 title:Run 1 created:0001-01-01 00:04:00Z updated:0001-01-01 00:02:00Z
3 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
4 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
5 - id:1 title:Run 1 created:0001-01-01 00:01:00Z updated:0001-01-01 00:05:00Z
";
        var txtFilteredList = @"Filtered list
0 - id:4 title:Run 1 created:0001-01-01 00:04:00Z updated:0001-01-01 00:02:00Z
1 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
2 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
";
        testOutput.Clear();
        Dump("Source list", list1);
        Assert.Equal(txtSourceList, testOutput.ToString());

#if DEBUG
        testOutput.Clear();
        Dump("Sorted internal list", col.DebugInternalList);
        Assert.Equal(txtInternalList, testOutput.ToString());
#endif

        testOutput.Clear();
        Dump("Filtered list", col);
        Assert.Equal(txtFilteredList, testOutput.ToString());

        Assert.Collection(col, new Action<Thing>[]
        {
            new Action<Thing>(t =>
            {
                Assert.Equal(list1[3], t);
            }),
            new Action<Thing>(t =>
            {
                Assert.Equal(list1[2], t);
            }),
            new Action<Thing>(t =>
            {
                Assert.Equal(list1[1], t);
            })
        });

        col.Dispose();
    }


    [Fact]
    public void OnlyTimesEqualOrHigherThan3Minutes()
    {
        var count = 6;

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(count - i) })).ToList();

        var col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) =>
                item.UpdatedAt >= now + TimeSpan.FromMinutes(3) && item.UpdatedAt <= now + TimeSpan.FromMinutes(5))
        {
            ProcessingDelay = TimeSpan.Zero
        };

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

        Assert.Equal(3, col.Count);

        var txtSourceList = @"Source list
0 - id:1 title:Run 1 created:0001-01-01 00:01:00Z updated:0001-01-01 00:05:00Z
1 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
2 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
3 - id:4 title:Run 1 created:0001-01-01 00:04:00Z updated:0001-01-01 00:02:00Z
4 - id:5 title:Run 1 created:0001-01-01 00:05:00Z updated:0001-01-01 00:01:00Z
5 - id:6 title:Run 1 created:0001-01-01 00:06:00Z updated:0001-01-01 00:00:00Z
";
        var txtInternalList = @"Sorted internal list
0 - id:6 title:Run 1 created:0001-01-01 00:06:00Z updated:0001-01-01 00:00:00Z
1 - id:5 title:Run 1 created:0001-01-01 00:05:00Z updated:0001-01-01 00:01:00Z
2 - id:4 title:Run 1 created:0001-01-01 00:04:00Z updated:0001-01-01 00:02:00Z
3 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
4 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
5 - id:1 title:Run 1 created:0001-01-01 00:01:00Z updated:0001-01-01 00:05:00Z
";
        var txtFilteredList = @"Filtered list
0 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
1 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
2 - id:1 title:Run 1 created:0001-01-01 00:01:00Z updated:0001-01-01 00:05:00Z
";
        testOutput.Clear();
        Dump("Source list", list1);
        Assert.Equal(txtSourceList, testOutput.ToString());
#if DEBUG
        testOutput.Clear();
        Dump("Sorted internal list", col.DebugInternalList);
        Assert.Equal(txtInternalList, testOutput.ToString());
#endif
        testOutput.Clear();
        Dump("Filtered list", col);
        Assert.Equal(txtFilteredList, testOutput.ToString());

        Assert.Collection(col,
            t =>
            {
                Assert.Equal(list1[2], t);
            },
            t =>
            {
                Assert.Equal(list1[1], t);
            },
            t =>
            {
                Assert.Equal(list1[0], t);
            });

        col.Dispose();
    }

    [Fact]
    public void OrderByDescendingNoFilter()
    {
        var count = 6;

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(count - i) })).ToList();
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 2", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(i) })).ToList();

        var col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare)
        {
            ProcessingDelay = TimeSpan.Zero
        };

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

        Assert.Equal(6, col.Count);

        var txtSourceList = @"Source list
0 - id:1 title:Run 1 created:0001-01-01 00:01:00Z updated:0001-01-01 00:05:00Z
1 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
2 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
3 - id:4 title:Run 1 created:0001-01-01 00:04:00Z updated:0001-01-01 00:02:00Z
4 - id:5 title:Run 1 created:0001-01-01 00:05:00Z updated:0001-01-01 00:01:00Z
5 - id:6 title:Run 1 created:0001-01-01 00:06:00Z updated:0001-01-01 00:00:00Z
";
        var txtInternalList = @"Sorted internal list
0 - id:1 title:Run 1 created:0001-01-01 00:01:00Z updated:0001-01-01 00:05:00Z
1 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
2 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
3 - id:4 title:Run 1 created:0001-01-01 00:04:00Z updated:0001-01-01 00:02:00Z
4 - id:5 title:Run 1 created:0001-01-01 00:05:00Z updated:0001-01-01 00:01:00Z
5 - id:6 title:Run 1 created:0001-01-01 00:06:00Z updated:0001-01-01 00:00:00Z
";
        var txtFilteredList = @"Filtered list
0 - id:1 title:Run 1 created:0001-01-01 00:01:00Z updated:0001-01-01 00:05:00Z
1 - id:2 title:Run 1 created:0001-01-01 00:02:00Z updated:0001-01-01 00:04:00Z
2 - id:3 title:Run 1 created:0001-01-01 00:03:00Z updated:0001-01-01 00:03:00Z
3 - id:4 title:Run 1 created:0001-01-01 00:04:00Z updated:0001-01-01 00:02:00Z
4 - id:5 title:Run 1 created:0001-01-01 00:05:00Z updated:0001-01-01 00:01:00Z
5 - id:6 title:Run 1 created:0001-01-01 00:06:00Z updated:0001-01-01 00:00:00Z
";
        testOutput.Clear();
        Dump("Source list", list1);
        Assert.Equal(txtSourceList, testOutput.ToString());

#if DEBUG
        testOutput.Clear();
        Dump("Sorted internal list", col.DebugInternalList);
        Assert.Equal(txtInternalList, testOutput.ToString());
#endif

        testOutput.Clear();
        Dump("Filtered list", col);
        Assert.Equal(txtFilteredList, testOutput.ToString());

        var k = 0;
        Assert.Collection(col, list1.Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(list1[k++], t);
        })).ToArray());

        count = 0;
        // add first items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.Equal(6, col.Count);

        col.Dispose();
    }

    [Fact]
    public void OrderByDescendingNoFilter1000()
    {
        var count = 1000;
        var total = 1000;

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(count - i) })).ToList();
        var list2 = new List<Thing>(Enumerable.Range(1, count).Select(i =>
            new Thing { Number = i, Title = "Run 2", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(count - i) })).ToList();

        var col = new TrackingCollection<Thing>(
            Observable.Never<Thing>(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare)
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Equal(total, col.Count);

        var k = 0;
        Assert.Collection(col, list1.Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(list1[k++], t);
        })).ToArray());

        count = 0;
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();
        Assert.Equal(total, col.Count);

        count = 0;
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();
        Assert.Equal(total, col.Count);

        col.Dispose();
    }


    [Fact(Skip = "Cannot run with other tests, timings go off")]
    public void ProcessingDelayPingsRegularly()
    {
        int count, total;
        count = total = 400;

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, count).Select(i => GetThing(i, i, count - i)).ToList());

        var col = new TrackingCollection<Thing>(
            list1.ToObservable().Delay(TimeSpan.FromMilliseconds(10)),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.ProcessingDelay = TimeSpan.FromMilliseconds(10);

        var sub = new Subject<Thing>();
        var times = new List<DateTimeOffset>();
        sub.Subscribe(t =>
        {
            times.Add(DateTimeOffset.UtcNow);
        });

        count = 0;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            sub.OnNext(t);
            if (++count == list1.Count)
            {
                sub.OnCompleted();
                evt.Set();
            }
        }, () => { });


        evt.WaitOne();
        evt.Reset();

        Assert.Equal(total, col.Count);

        var k = 0;
        Assert.Collection(col, list1.Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(list1[k++], t);
        })).ToArray());


        long totalTime = 0;

        for (var j = 1; j < times.Count; j++)
            totalTime += (times[j] - times[j - 1]).Ticks;
        var avg = TimeSpan.FromTicks(totalTime / times.Count).TotalMilliseconds;
        Assert.InRange(avg, 9, 12);
        col.Dispose();
    }

    [Fact]
    public void NotInitializedCorrectlyThrows1()
    {
        var col = new TrackingCollection<Thing>(OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        Assert.Throws<InvalidOperationException>(() => col.Subscribe());
    }

    [Fact]
    public void NotInitializedCorrectlyThrows2()
    {
        var col = new TrackingCollection<Thing>(OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        Assert.Throws<InvalidOperationException>(() => col.Subscribe(_ => { }, () => { }));
    }

    [Fact]
    public void NoChangingAfterDisposed1()
    {
        var col = new TrackingCollection<Thing>(Observable.Never<Thing>(), OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.Dispose();
        Assert.Throws<ObjectDisposedException>(() => col.AddItem(new Thing()));
    }

    [Fact]
    public void NoChangingAfterDisposed2()
    {
        var col = new TrackingCollection<Thing>(Observable.Never<Thing>(), OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);
        col.Dispose();
        Assert.Throws<ObjectDisposedException>(() => col.RemoveItem(new Thing()));
    }


    [Fact]
    public void FilterTitleRun2()
    {
        var count = 0;
        var total = 1000;

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, total).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(i) })).ToList();
        var list2 = new List<Thing>(Enumerable.Range(1, total).Select(i =>
            new Thing { Number = i, Title = "Run 2", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(i) })).ToList();

        var col = new TrackingCollection<Thing>(
            list1.ToObservable(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => item.Title.Equals("Run 2"))
        {
            ProcessingDelay = TimeSpan.Zero
        };

        count = 0;

        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        evt.WaitOne();
        evt.Reset();

        Assert.Equal(total, count);
        Assert.Equal(0, col.Count);

        count = 0;

        // add new items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.Equal(total, count);
        Assert.Equal(total, col.Count);

        Assert.Collection(col, list2.Reverse<Thing>().Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(x, t);
        })).ToArray());

        col.Dispose();
    }

    [Fact]
    public void OrderByDoesntMatchOriginalOrderTimings()
    {
        var count = 0;
        var total = 1000;

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, total).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(i) })).ToList();
        var list2 = new List<Thing>(Enumerable.Range(1, total).Select(i =>
            new Thing { Number = i, Title = "Run 2", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(i) })).ToList();

        var col = new TrackingCollection<Thing>(
            list1.ToObservable(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => item.Title.Equals("Run 2"))
        {
            ProcessingDelay = TimeSpan.Zero
        };

        count = 0;
        var evt = new ManualResetEvent(false);
        var start = DateTimeOffset.UtcNow;
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        evt.WaitOne();
        var time = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
        Dump($"time: {time}");
        //Assert.InRange(time, 0, 100);
        evt.Reset();

        Assert.Equal(total, count);
        Assert.Equal(0, col.Count);

        count = 0;

        start = DateTimeOffset.UtcNow;
        // add new items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        time = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
        Dump($"time: {time}");
        //Assert.InRange(time, 0, 200);
        evt.Reset();

        Assert.Equal(total, count);
        Assert.Equal(total, col.Count);

        Assert.Collection(col, list2.Reverse<Thing>().Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(x, t);
        })).ToArray());

        col.Dispose();
    }

    [Fact]
    public void OrderByMatchesOriginalOrderTimings()
    {
        var count = 0;
        var total = 1000;

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var list1 = new List<Thing>(Enumerable.Range(1, total).Select(i =>
            new Thing { Number = i, Title = "Run 1", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(total - i) })).ToList();
        var list2 = new List<Thing>(Enumerable.Range(1, total).Select(i =>
            new Thing { Number = i, Title = "Run 2", CreatedAt = now + TimeSpan.FromMinutes(i), UpdatedAt = now + TimeSpan.FromMinutes(total - i) })).ToList();

        var col = new TrackingCollection<Thing>(
            list1.ToObservable(),
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => item.Title.Equals("Run 2"))
        {
            ProcessingDelay = TimeSpan.Zero
        };

        count = 0;
        var evt = new ManualResetEvent(false);
        col.Subscribe(t =>
        {
            if (++count == list1.Count)
                evt.Set();
        }, () => { });

        evt.WaitOne();
        evt.Reset();

        Assert.Equal(total, count);
        Assert.Equal(0, col.Count);

        count = 0;

        // add new items
        foreach (var l in list2)
            col.AddItem(l);

        evt.WaitOne();
        evt.Reset();

        Assert.Equal(total, count);
        Assert.Equal(total, col.Count);

        Assert.Collection(col, list2.Select(x => new Action<Thing>(t =>
        {
            Assert.Equal(x, t);
        })).ToArray());

        col.Dispose();
    }

    [Fact]
    public void SortingTest()
    {
        var source = new Subject<Thing>();

        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare)
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col, new Action<Thing>[] {
            new Action<Thing>(t => Assert.True(Compare(t, GetThing(1, 10)))),
        });

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 2))));

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:12,0:0:10}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))));

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:12,0:0:11,0:0:10}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))));

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:13,0:0:12,0:0:11,0:0:10,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(5, 14))),
            t => Assert.True(Compare(t, GetThing(6, 13))),
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(4, 5))));

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:13,0:0:12,0:0:11,0:0:10,0:0:5,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 13))),
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5))));

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:13,0:0:12,0:0:11,0:0:10,0:0:5,0:0:4}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5))),
            t => Assert.True(Compare(t, GetThing(6, 4))));

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:13,0:0:12,0:0:11,0:0:10,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))), t => Assert.True(Compare(t, GetThing(4, 5))));

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:10,0:0:5,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5))));

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:10,0:0:9,0:0:5,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5))));

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:10,0:0:9,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(5, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(4, 5))));

        col.RemoveItem(GetThing(1, 10));
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:9,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(5, 12))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        col.Dispose();
    }

    [Fact]
    public void SortingTestWithFilterTrue()
    {
        var source = new Subject<Thing>();

        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => true)
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col, new Action<Thing>[] {
            new Action<Thing>(t => Assert.True(Compare(t, GetThing(1, 10)))),
        });

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 2)))
        );

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:12,0:0:10}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:12,0:0:11,0:0:10}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:13,0:0:12,0:0:11,0:0:10,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(5, 14))),
            t => Assert.True(Compare(t, GetThing(6, 13))),
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:13,0:0:12,0:0:11,0:0:10,0:0:5,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 13))),
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:13,0:0:12,0:0:11,0:0:10,0:0:5,0:0:4}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5))),
            t => Assert.True(Compare(t, GetThing(6, 4)))
        );

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:13,0:0:12,0:0:11,0:0:10,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:10,0:0:5,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:10,0:0:9,0:0:5,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:10,0:0:9,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(5, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        col.RemoveItem(GetThing(1, 10));
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:9,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 14))),
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(5, 12))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        col.Dispose();
    }

    [Fact]
    public void SortingTestWithFilterBetween6And12()
    {
        var source = new Subject<Thing>();

        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt.Minute >= 6 && item.UpdatedAt.Minute <= 12)
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9)))
        );

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(5, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9)))
        );

        col.RemoveItem(GetThing(1, 10));
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(5, 12))),
            t => Assert.True(Compare(t, GetThing(2, 9)))
        );

        col.Dispose();
    }


    [Fact]
    public void SortingTestWithFilterPosition2to4()
    {
        var source = new Subject<Thing>();

        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => position >= 2 && position <= 4)
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col);

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        Assert.Collection(col);

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col);

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col, t => Assert.True(Compare(t, GetThing(1, 10))));

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5)))
        );

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5)))
        );

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5)))
        );

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(5, 5)))
        );

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(5, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9)))
        );

        col.RemoveItem(GetThing(1, 10));
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(5, 12))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        col.Dispose();
    }


    [Fact]
    public void SortingTestWithFilterPosition1And3to4()
    {
        var source = new Subject<Thing>();

        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare,
            (item, position, list) => position == 1 || (position >= 3 && position <= 4))
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col);

        // testing ADD
        // add another thing with UpdatedAt=0:0:2
        expectedCount = 2;
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        // check that list has {0:0:10,0:0:2}
        Assert.Collection(col, t => Assert.True(Compare(t, GetThing(2, 2))));

        // testing MOVE
        // replace thing with UpdatedAt=0:0:2 to UpdatedAt=0:0:12
        expectedCount = 3;
        Add(source, GetThing(2, 12));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col, t => Assert.True(Compare(t, GetThing(1, 10))));

        // testing INSERT
        expectedCount = 4;
        Add(source, GetThing(3, 11));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col, t => Assert.True(Compare(t, GetThing(3, 11))));

        // testing INSERT
        expectedCount = 7;
        Add(source, GetThing(4, 5));
        Add(source, GetThing(5, 14));
        Add(source, GetThing(6, 13));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(6, 13))),
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(1, 10)))
        );

        // testing MOVE from top to middle
        expectedCount = 8;
        Add(source, GetThing(5, 5));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5)))
        );

        // testing MOVE from top to bottom
        expectedCount = 9;
        Add(source, GetThing(6, 4));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 11))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        // testing MOVE from bottom to top
        expectedCount = 10;
        Add(source, GetThing(6, 14));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 12))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5)))
        );

        // testing MOVE from middle bottom to middle top
        expectedCount = 11;
        Add(source, GetThing(3, 14));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(5, 5)))
        );

        // testing MOVE from middle top to middle bottom
        expectedCount = 12;
        Add(source, GetThing(2, 9));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(5, 5)))
        );

        // testing MOVE from middle bottom to middle top more than 1 position
        expectedCount = 13;
        Add(source, GetThing(5, 12));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 14))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9)))
        );

        expectedCount = 14;
        Add(source, GetThing(3, 13));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 13))),
            t => Assert.True(Compare(t, GetThing(1, 10))),
            t => Assert.True(Compare(t, GetThing(2, 9)))
        );

        col.RemoveItem(GetThing(1, 10));
        // check that list has {0:0:14,0:0:14,0:0:12,0:0:9,0:0:5}
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 13))),
            t => Assert.True(Compare(t, GetThing(2, 9))),
            t => Assert.True(Compare(t, GetThing(4, 5)))
        );

        col.Dispose();
    }


    [Fact]
    public void SortingTestWithFilterMoves()
    {
        var source = new Subject<Thing>();

        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => (position >= 1 && position <= 2) || (position >= 5 && position <= 7))
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 3))),
            t => Assert.True(Compare(t, GetThing(3, 5))),
            t => Assert.True(Compare(t, GetThing(6, 11))),
            t => Assert.True(Compare(t, GetThing(7, 13))),
            t => Assert.True(Compare(t, GetThing(8, 15)))
        );


        expectedCount = 10;
        Add(source, GetThing(7, 4));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(2, 3))),
            t => Assert.True(Compare(t, GetThing(7, 4))),
            t => Assert.True(Compare(t, GetThing(5, 9))),
            t => Assert.True(Compare(t, GetThing(6, 11))),
            t => Assert.True(Compare(t, GetThing(8, 15)))
        );

        expectedCount = 11;
        Add(source, GetThing(9, 2));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(9, 2))),
            t => Assert.True(Compare(t, GetThing(2, 3))),
            t => Assert.True(Compare(t, GetThing(4, 7))),
            t => Assert.True(Compare(t, GetThing(5, 9))),
            t => Assert.True(Compare(t, GetThing(6, 11)))
        );

        col.Dispose();
    }

    [Fact]
    public void ChangingItemContentRemovesItFromFilteredList()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.CreatedAt).Compare,
            (item, position, list) => item.UpdatedAt < now + TimeSpan.FromMinutes(6))
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 1, 1))),
            t => Assert.True(Compare(t, GetThing(3, 3, 3))),
            t => Assert.True(Compare(t, GetThing(5, 5, 5)))
        );

        expectedCount = 6;
        Add(source, GetThing(5, 5, 6));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 1, 1))),
            t => Assert.True(Compare(t, GetThing(3, 3, 3)))
        );
        col.Dispose();
    }

    [Fact]
    public void ChangingItemContentRemovesItFromFilteredList2()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.CreatedAt).Compare,
            (item, position, list) =>
                item.UpdatedAt > now + TimeSpan.FromMinutes(2) && item.UpdatedAt < now + TimeSpan.FromMinutes(8))
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 3, 3))),
            t => Assert.True(Compare(t, GetThing(5, 5, 5))),
            t => Assert.True(Compare(t, GetThing(7, 7, 7)))
        );

        expectedCount = 6;
        Add(source, GetThing(7, 7, 8));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 3, 3))),
            t => Assert.True(Compare(t, GetThing(5, 5, 5)))
        );

        expectedCount = 7;
        Add(source, GetThing(7, 7, 7));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 3, 3))),
            t => Assert.True(Compare(t, GetThing(5, 5, 5))),
            t => Assert.True(Compare(t, GetThing(7, 7, 7)))
        );

        expectedCount = 8;
        Add(source, GetThing(3, 3, 2));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(5, 5, 5))),
            t => Assert.True(Compare(t, GetThing(7, 7, 7)))
        );

        expectedCount = 9;
        Add(source, GetThing(3, 3, 3));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 3, 3))),
            t => Assert.True(Compare(t, GetThing(5, 5, 5))),
            t => Assert.True(Compare(t, GetThing(7, 7, 7)))
        );

        expectedCount = 10;
        Add(source, GetThing(5, 5, 1));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 3, 3))),
            t => Assert.True(Compare(t, GetThing(7, 7, 7)))
        );
        col.Dispose();
    }

    [Fact]
    public void ChangingFilterUpdatesCollection()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt < now + TimeSpan.FromMinutes(10))
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 1))),
            t => Assert.True(Compare(t, GetThing(2, 2))),
            t => Assert.True(Compare(t, GetThing(3, 3))),
            t => Assert.True(Compare(t, GetThing(4, 4))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(6, 6))),
            t => Assert.True(Compare(t, GetThing(7, 7))),
            t => Assert.True(Compare(t, GetThing(8, 8))),
            t => Assert.True(Compare(t, GetThing(9, 9)))
        );

        col.SetFilter((item, position, list) => item.UpdatedAt < now + TimeSpan.FromMinutes(8));

        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 1))),
            t => Assert.True(Compare(t, GetThing(2, 2))),
            t => Assert.True(Compare(t, GetThing(3, 3))),
            t => Assert.True(Compare(t, GetThing(4, 4))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(6, 6))),
            t => Assert.True(Compare(t, GetThing(7, 7)))
        );
        col.Dispose();
    }

    [Fact]
    public void ChangingSortUpdatesCollection()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt < now + TimeSpan.FromMinutes(10))
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 1))),
            t => Assert.True(Compare(t, GetThing(2, 2))),
            t => Assert.True(Compare(t, GetThing(3, 3))),
            t => Assert.True(Compare(t, GetThing(4, 4))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(6, 6))),
            t => Assert.True(Compare(t, GetThing(7, 7))),
            t => Assert.True(Compare(t, GetThing(8, 8))),
            t => Assert.True(Compare(t, GetThing(9, 9)))
        );

        col.SetComparer(OrderedComparer<Thing>.OrderByDescending(x => x.UpdatedAt).Compare);

        Assert.Collection(col, new Action<Thing>[] {
            (t => Assert.True(Compare(t, GetThing(1, 1)))),
            (t => Assert.True(Compare(t, GetThing(2, 2)))),
            (t => Assert.True(Compare(t, GetThing(3, 3)))),
            (t => Assert.True(Compare(t, GetThing(4, 4)))),
            (t => Assert.True(Compare(t, GetThing(5, 5)))),
            (t => Assert.True(Compare(t, GetThing(6, 6)))),
            (t => Assert.True(Compare(t, GetThing(7, 7)))),
            (t => Assert.True(Compare(t, GetThing(8, 8)))),
            (t => Assert.True(Compare(t, GetThing(9, 9))))
        }.Reverse().ToArray());
        col.Dispose();
    }

    [Fact]
    public void AddingItemsToCollectionManuallyThrows()
    {
        var col = new TrackingCollection<Thing>(Observable.Empty<Thing>());
        Assert.Throws<InvalidOperationException>(() => col.Add(GetThing(1)));
        col.Dispose();
    }

    [Fact]
    public void InsertingItemsIntoCollectionManuallyThrows()
    {
        var col = new TrackingCollection<Thing>(Observable.Empty<Thing>());
        Assert.Throws<InvalidOperationException>(() => col.Insert(0, GetThing(1)));
        col.Dispose();
    }

    [Fact]
    public void MovingItemsIntoCollectionManuallyThrows()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt < now + TimeSpan.FromMinutes(10))
        {
            ProcessingDelay = TimeSpan.Zero
        };

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 2;
        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        Assert.Throws<InvalidOperationException>(() => col.Move(0, 1));
        col.Dispose();
    }

    [Fact]
    public void RemovingItemsFromCollectionManuallyThrows()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt < now + TimeSpan.FromMinutes(10))
        {
            ProcessingDelay = TimeSpan.Zero
        };

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 2;
        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        Assert.Throws<InvalidOperationException>(() => col.Remove(GetThing(1)));
        col.Dispose();
    }

    [Fact]
    public void RemovingItemsFromCollectionManuallyThrows2()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => item.UpdatedAt < now + TimeSpan.FromMinutes(10))
        {
            ProcessingDelay = TimeSpan.Zero
        };

        var count = 0;
        var expectedCount = 0;
        var evt = new ManualResetEvent(false);

        col.Subscribe(t =>
        {
            if (++count == expectedCount)
                evt.Set();
        }, () => { });

        expectedCount = 2;
        Add(source, GetThing(1, 1));
        Add(source, GetThing(2, 2));
        evt.WaitOne();
        evt.Reset();
        Assert.Throws<InvalidOperationException>(() => col.RemoveAt(0));
        col.Dispose();
    }


    [Fact]
    public void ChangingComparers()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.CreatedAt).Compare);
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
        Add(source, GetThing(1, 1, 9));
        Add(source, GetThing(2, 2, 8));
        Add(source, GetThing(3, 3, 7));
        Add(source, GetThing(4, 4, 6));
        Add(source, GetThing(5, 5, 4));
        Add(source, GetThing(6, 6, 5));
        Add(source, GetThing(7, 7, 3));
        Add(source, GetThing(8, 8, 2));
        Add(source, GetThing(9, 9, 1));
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(1, 1, 9))),
            t => Assert.True(Compare(t, GetThing(2, 2, 8))),
            t => Assert.True(Compare(t, GetThing(3, 3, 7))),
            t => Assert.True(Compare(t, GetThing(4, 4, 6))),
            t => Assert.True(Compare(t, GetThing(5, 5, 4))),
            t => Assert.True(Compare(t, GetThing(6, 6, 5))),
            t => Assert.True(Compare(t, GetThing(7, 7, 3))),
            t => Assert.True(Compare(t, GetThing(8, 8, 2))),
            t => Assert.True(Compare(t, GetThing(9, 9, 1)))
        );

        col.SetComparer(null);
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(9, 9, 1))),
            t => Assert.True(Compare(t, GetThing(8, 8, 2))),
            t => Assert.True(Compare(t, GetThing(7, 7, 3))),
            t => Assert.True(Compare(t, GetThing(5, 5, 4))),
            t => Assert.True(Compare(t, GetThing(6, 6, 5))),
            t => Assert.True(Compare(t, GetThing(4, 4, 6))),
            t => Assert.True(Compare(t, GetThing(3, 3, 7))),
            t => Assert.True(Compare(t, GetThing(2, 2, 8))),
            t => Assert.True(Compare(t, GetThing(1, 1, 9)))
        );
        col.Dispose();
    }


    [Fact]
    public void Removing()
    {
        var source = new Subject<Thing>();

        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var col = new TrackingCollection<Thing>(
            source,
            OrderedComparer<Thing>.OrderBy(x => x.UpdatedAt).Compare,
            (item, position, list) => (position > 2 && position < 5) || (position > 6 && position < 8))
        {
            ProcessingDelay = TimeSpan.Zero
        };

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
        evt.WaitOne();
        evt.Reset();
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(3, 3))),
            t => Assert.True(Compare(t, GetThing(4, 4))),
            t => Assert.True(Compare(t, GetThing(7, 7))));

        col.RemoveItem(GetThing(2));
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(4, 4))),
            t => Assert.True(Compare(t, GetThing(5, 5))),
            t => Assert.True(Compare(t, GetThing(8, 8))));

        col.RemoveItem(GetThing(5));
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(4, 4))),
            t => Assert.True(Compare(t, GetThing(6, 6))),
            t => Assert.True(Compare(t, GetThing(9, 9)))
        );

        col.RemoveItem(GetThing(100));
        Assert.Collection(col,
            t => Assert.True(Compare(t, GetThing(4, 4))),
            t => Assert.True(Compare(t, GetThing(6, 6))),
            t => Assert.True(Compare(t, GetThing(9, 9)))
        );
        col.Dispose();
    }

    [Fact]
    public void DisposingThrows()
    {
        var col = new TrackingCollection<Thing>(Observable.Empty<Thing>());
        col.Dispose();
        Assert.Throws<ObjectDisposedException>(() => col.SetFilter(null));
        Assert.Throws<ObjectDisposedException>(() => col.SetComparer(null));
        Assert.Throws<ObjectDisposedException>(() => col.Subscribe());
        Assert.Throws<ObjectDisposedException>(() => col.AddItem(GetThing(1)));
        Assert.Throws<ObjectDisposedException>(() => col.RemoveItem(GetThing(1)));
    }

    [Fact(Skip = "Getting a stackoverflow randomly on this test")]
    public void MultipleSortingAndFiltering()
    {
        var expectedTotal = 20;
        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        var rnd = new Random(214748364);

        var titles1 = Enumerable.Range(1, expectedTotal).Select(x => ((char)('a' + x)).ToString()).ToList();
        var dates1 = Enumerable.Range(1, expectedTotal).Select(x => now + TimeSpan.FromMinutes(x)).ToList();

        var idstack1 = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));
        var datestack1 = new Stack<DateTimeOffset>(dates1);
        var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

        var titles2 = Enumerable.Range(1, expectedTotal).Select(x => ((char)('c' + x)).ToString()).ToList();
        var dates2 = Enumerable.Range(1, expectedTotal).Select(x => now + TimeSpan.FromMinutes(x)).ToList();

        var idstack2 = new Stack<int>(Enumerable.Range(1, expectedTotal).OrderBy(rnd.Next));
        var datestack2 = new Stack<DateTimeOffset>(new List<DateTimeOffset>() {
                dates2[2],
                dates2[0],
                dates2[1],
                dates2[3],
                dates2[5],
                dates2[9],
                dates2[15],
                dates2[6],
                dates2[7],
                dates2[8],
                dates2[13],
                dates2[10],
                dates2[16],
                dates2[11],
                dates2[12],
                dates2[14],
                dates2[17],
                dates2[18],
                dates2[19],
                dates2[4],
            });
        var titlestack2 = new Stack<string>(titles2.OrderBy(_ => rnd.Next()));

        var list1 = Observable.Defer(() => Enumerable.Range(1, expectedTotal)
            .OrderBy(rnd.Next)
            .Select(x =>
            {
                var id = idstack1.Pop();
                var date = datestack1.Pop();
                var title = titlestack1.Pop();
                return new Thing(id, title, date, date);
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

                return new Thing(id, title, date, date);
            })
            .ToObservable())
            .Replay()
            .RefCount();

        var col = new TrackingCollection<Thing>(
            list1.Concat(list2),
            OrderedComparer<Thing>.OrderByDescending(x => x.CreatedAt).Compare,
            (item, idx, list) => idx < 5
        );

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

        Assert.Collection(col, dates2.Select(
            x => new Action<Thing>(t => Assert.Equal(x, t.CreatedAt))).Reverse().Take(5).ToArray());

        col.SetComparer(OrderedComparer<Thing>.OrderBy(x => x.Number).Compare);
        Assert.Collection(col, Enumerable.Range(1, expectedTotal)
            .Select(x => new Action<Thing>(t => Assert.Equal(x, t.Number))).Take(5).ToArray());

        col.SetComparer(OrderedComparer<Thing>.OrderBy(x => x.CreatedAt).Compare);
        Assert.Collection(col, dates2.Select(
            x => new Action<Thing>(t => Assert.Equal(x, t.CreatedAt))).Take(5).ToArray());

        col.SetComparer(OrderedComparer<Thing>.OrderByDescending(x => x.Title).Compare);
        Assert.Collection(col, titles2.Select(
            x => new Action<Thing>(t => Assert.Equal(x, t.Title))).Reverse().Take(5).ToArray());

        col.SetComparer(OrderedComparer<Thing>.OrderBy(x => x.Title).Compare);
        Assert.Collection(col, titles2.Select(
            x => new Action<Thing>(t => Assert.Equal(x, t.Title))).Take(5).ToArray());

        col.Dispose();
    }
}
