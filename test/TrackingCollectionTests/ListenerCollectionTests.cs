using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GitHub.Collections;
using NUnit.Framework;
using ReactiveUI.Testing;

[TestFixture]
public class ListenerCollectionTests : TestBase
{
    [Test]
    public void StickyItemShouldNotBePresentInitiallyWhereNoSelectionHasHappened()
    {
        var source = CreateSource();
        var stickie = new Thing();
        var selection = Observable.Empty<Thing>();
        var target = source.CreateListenerCollection(stickie, selection);

        CollectionAssert.AreEqual(source, target);
    }

    [Test]
    public void StickyItemShouldNotBePresentAfterCreationWhenSelectionNull()
    {
        var source = CreateSource();
        var stickie = new Thing();
        var selection = Observable.Return<Thing>(null);
        var target = source.CreateListenerCollection(stickie, selection);

        CollectionAssert.AreEqual(source, target);
    }

    [Test]
    public void StickyItemShouldBePresentAfterCreationWhenSelectionNotNull()
    {
        var source = CreateSource();
        var stickie = new Thing();
        var selection = Observable.Return(source[0]);
        var target = source.CreateListenerCollection(stickie, selection);

        var expected = new[] { stickie }.Concat(source);
        CollectionAssert.AreEqual(expected, target);
    }

    [Test]
    public void StickyItemShouldNotBePresentAfterCreationWhenSelectionIsStickyItem()
    {
        var source = CreateSource();
        var stickie = new Thing();
        var selection = Observable.Return(stickie);
        var target = source.CreateListenerCollection(stickie, selection);

        CollectionAssert.AreEqual(source, target);
    }

    [Test]
    public void StickyItemShouldNotBePresentAfterCreationWhenSelectionEqualsStickyItem()
    {
        var source = CreateSource();
        var stickie = new Thing();
        var selection = Observable.Return(new Thing());
        var target = source.CreateListenerCollection(stickie, selection);

        CollectionAssert.AreEqual(source, target);
    }

    [Test]
    public void StickyItemShouldBeAddedWhenSelectionChangesFromNull()
    {
        var source = CreateSource();
        var selection = new BehaviorSubject<Thing>(null);
        var stickie = new Thing();
        var target = source.CreateListenerCollection(stickie, selection);

        CollectionAssert.AreEqual(source, target);

        selection.OnNext(source[0]);

        var expected = new[] { stickie }.Concat(source);
        CollectionAssert.AreEqual(expected, target);
    }

    [Test]
    public void StickyItemShouldBeRemovedWhenSelectionChangesToNull()
    {
        var source = CreateSource();
        var stickie = new Thing();
        var selection = new BehaviorSubject<Thing>(source[0]);
        var target = source.CreateListenerCollection(stickie, selection);

        var expected = new[] { stickie }.Concat(source);
        CollectionAssert.AreEqual(expected, target);

        selection.OnNext(null);

        CollectionAssert.AreEqual(source, target);
    }

    [Test]
    public void StickyItemShouldBeRemovedWhenSelectionChangesToStickyItem()
    {
        var source = CreateSource();
        var stickie = new Thing();
        var selection = new BehaviorSubject<Thing>(source[0]);
        var target = source.CreateListenerCollection(stickie, selection);

        var expected = new[] { stickie }.Concat(source);
        CollectionAssert.AreEqual(expected, target);

        selection.OnNext(stickie);

        CollectionAssert.AreEqual(source, target);
    }

    [Test]
    public void ResetingTrackingCollectionWorks()
    {
        var source = CreateSource();
        var stickie = new Thing();
        var selection = new ReplaySubject<Thing>();
        var target = source.CreateListenerCollection(stickie, selection);
        selection.OnNext(stickie);
        selection.OnNext(null);
        CollectionAssert.AreEqual(source, target);
        source.Filter = (a, b, c) => true;
        CollectionAssert.AreEqual(source, target);
    }

    static TrackingCollection<Thing> CreateSource()
    {
        var result = new TrackingCollection<Thing>(Observable.Empty<Thing>());
        result.Subscribe();
        result.AddItem(new Thing(1, "item1", DateTimeOffset.MinValue));
        result.AddItem(new Thing(2, "item2", DateTimeOffset.MinValue));
        result.AddItem(new Thing(3, "item3", DateTimeOffset.MinValue));
        return result;
    }
}
