using GitHub;
using GitHub.Collections;
using GitHub.Helpers;
using GitHub.Models;
using NSubstitute;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnitTests;
using Xunit;

public class TrackingCollectionTests
{
    public class Sorting
    {
        [Fact]
        public void SortingAscending()
        {
            var rnd = new Random();
            var list = Enumerable.Range(1, 20)
                .OrderBy(rnd.Next)
                .Select(x => {
                    var s = Substitute.For<IPullRequestModel>();
                    s.Number.Returns(x);
                    return s;
                })
                .ToObservable();
            var col = new TrackingCollection<IPullRequestModel>(list);
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            col.Subscribe();
            Assert.Collection(col, Enumerable.Range(1, 20).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).ToArray());
        }

        [Fact]
        public void SortingRandomizedDescending()
        {
            var rnd = new Random();
            var list = Enumerable.Range(1, 20)
                .OrderBy(rnd.Next)
                .Select(x => {
                    var s = Substitute.For<IPullRequestModel>();
                    s.Number.Returns(x);
                    return s;
                })
                .ToObservable();
            var col = new TrackingCollection<IPullRequestModel>(list);
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.Number).Compare);
            col.Subscribe();
            Assert.Collection(col, Enumerable.Range(1, 20).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).Reverse().ToArray());
        }


        [Fact]
        public void CollectionIsSortedWhenUpdated()
        {
            var rnd = new Random();
            var list = Observable.Defer(() =>
                Enumerable.Range(1, 20)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var s = Substitute.For<IPullRequestModel>();
                    s.Number.Returns(x);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == x);
                    s.When(c => c.CopyFrom(Arg.Any<IPullRequestModel>())).Do(c =>
                    {
                        var nbr = c.Arg<IPullRequestModel>().Number;
                        var str = c.Arg<IPullRequestModel>().Title;
                        s.Number.Returns(nbr);
                        s.Title.Returns(str);
                    });
                    return s;
                })
                .ToObservable()
            );
            var list1 = list.Do(s => s.Title.Returns("Cache"));
            var list2 = list.Do(s => s.Title.Returns("Live"));
            var col = new TrackingCollection<IPullRequestModel>(list1.Concat(list2));
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            col.Subscribe();
            var inspectors = Enumerable.Range(1, 20).Select(x => new Action<IPullRequestModel>(t =>
            {
                Assert.Equal(x, t.Number);
                Assert.Equal("Live", t.Title);
            }));
            Assert.Collection(col, inspectors.ToArray());
        }

        [Fact]
        public void CollectionIsSortedWhenTitleUpdated()
        {
            var rnd = new Random();
            var list1 = Observable.Defer(() => Enumerable.Range(1, 20)
                .OrderBy(x => rnd.Next(1, 20))
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
            var list2 = Observable.Defer(() => Enumerable.Range(1, 20)
                .OrderBy(x => rnd.Next(1, 20))
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
            var col = new TrackingCollection<IPullRequestModel>(list1.Concat(list2));
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Title).Compare);
            col.Subscribe();
            Assert.NotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());
            Assert.Collection(col, Enumerable.Range(1, 20).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(((char)('c' + x)).ToString(), t.Title))).ToArray());
        }

        [Fact]
        public void CollectionIsSortedWhenComparerUpdated()
        {
            var rnd = new Random();
            var titles1 = Enumerable.Range(1, 20).Select(x => ((char)('a' + x)).ToString()).ToList();
            var idstack1 = new Stack<int>(Enumerable.Range(1, 20).OrderBy(rnd.Next));
            var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

            var titles2 = Enumerable.Range(1, 20).Select(x => ((char)('c' + x)).ToString()).ToList();
            var idstack2 = new Stack<int>(Enumerable.Range(1, 20).OrderBy(rnd.Next));
            var titlestack2 = new Stack<string>(titles2.OrderBy(_ => rnd.Next()));

            var list1 = Observable.Defer(() => Enumerable.Range(1, 20)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack1.Pop();
                    var title = titlestack1.Pop();

                    var s = Substitute.For<IPullRequestModel>();
                    s.Title.Returns(title);
                    s.Number.Returns(id);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == id);
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

            var list2 = Observable.Defer(() => Enumerable.Range(1, 20)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack2.Pop();
                    var title = titlestack2.Pop();

                    var s = Substitute.For<IPullRequestModel>();
                    s.Title.Returns(title);
                    s.Number.Returns(id);
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == id);
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var col = new TrackingCollection<IPullRequestModel>(list1.Concat(list2));
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Title).Compare);
            col.Subscribe();
            Assert.NotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());
            Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            Assert.Collection(col, Enumerable.Range(1, 20).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).ToArray());
        }


        [Fact]
        public void SortingInPlace()
        {
            var rnd = new Random();

            var titles1 = Enumerable.Range(1, 20).Select(x => ((char)('a' + x)).ToString()).ToList();
            var dates1 = Enumerable.Range(1, 20).Select(x => DateTimeOffset.Now + TimeSpan.FromMinutes(x)).ToList();

            var idstack1 = new Stack<int>(Enumerable.Range(1, 20).OrderBy(rnd.Next));
            var datestack1 = new Stack<DateTimeOffset>(dates1.OrderBy(_ => rnd.Next()));
            var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

            var titles2 = Enumerable.Range(1, 20).Select(x => ((char)('c' + x)).ToString()).ToList();
            var dates2 = Enumerable.Range(1, 20).Select(x => DateTimeOffset.Now + TimeSpan.FromMinutes(x)).ToList();

            var idstack2 = new Stack<int>(Enumerable.Range(1, 20).OrderBy(rnd.Next));
            var datestack2 = new Stack<DateTimeOffset>(dates2.OrderBy(_ => rnd.Next()));
            var titlestack2 = new Stack<string>(titles2.OrderBy(_ => rnd.Next()));

            var list1 = Observable.Defer(() => Enumerable.Range(1, 20)
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

            var list2 = Observable.Defer(() => Enumerable.Range(1, 20)
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

            var col = new TrackingCollection<IPullRequestModel>(list1.Concat(list2));
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.CreatedAt).Compare);
            col.Subscribe();

            // it's initially sorted by date, so id list should not match
            Assert.NotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());
            Assert.Collection(col, dates2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.CreatedAt))).ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            Assert.Collection(col, Enumerable.Range(1, 20).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Title).Compare);
            Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).ToArray());
        }

        [Fact]
        public void MultipleSortingWithDifferentDirections()
        {
            var rnd = new Random();

            var titles1 = Enumerable.Range(1, 20).Select(x => ((char)('a' + x)).ToString()).ToList();
            var dates1 = Enumerable.Range(1, 20).Select(x => DateTimeOffset.Now + TimeSpan.FromMinutes(x)).ToList();

            var idstack1 = new Stack<int>(Enumerable.Range(1, 20).OrderBy(rnd.Next));
            var datestack1 = new Stack<DateTimeOffset>(dates1.OrderBy(_ => rnd.Next()));
            var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

            var titles2 = Enumerable.Range(1, 20).Select(x => ((char)('c' + x)).ToString()).ToList();
            var dates2 = Enumerable.Range(1, 20).Select(x => DateTimeOffset.Now + TimeSpan.FromMinutes(x)).ToList();

            var idstack2 = new Stack<int>(Enumerable.Range(1, 20).OrderBy(rnd.Next));
            var datestack2 = new Stack<DateTimeOffset>(dates2.OrderBy(_ => rnd.Next()));
            var titlestack2 = new Stack<string>(titles2.OrderBy(_ => rnd.Next()));

            var list1 = Observable.Defer(() => Enumerable.Range(1, 20)
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

            var list2 = Observable.Defer(() => Enumerable.Range(1, 20)
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

            var col = new TrackingCollection<IPullRequestModel>(list1.Concat(list2));
            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.CreatedAt).Compare);
            col.Subscribe();

            // it's initially sorted by date, so id list should not match
            Assert.NotEqual(list1.Select(x => x.Number).ToEnumerable(), list2.Select(x => x.Number).ToEnumerable());

            Assert.Collection(col, dates2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.CreatedAt))).Reverse().ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            Assert.Collection(col, Enumerable.Range(1, 20).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.CreatedAt).Compare);
            Assert.Collection(col, dates2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.CreatedAt))).ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.Title).Compare);
            Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).Reverse().ToArray());

            col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Title).Compare);
            Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).ToArray());
        }

        [Fact]
        public void MultipleSortingDerivedList()
        {
            var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
            var count = 20;
            var rnd = new Random();

            var titles1 = Enumerable.Range(1, count).Select(x => ((char)('a' + x)).ToString()).ToList();
            var dates1 = Enumerable.Range(1, count).Select(x => now + TimeSpan.FromMinutes(x)).ToList();

            var idstack1 = new Stack<int>(Enumerable.Range(1, count).OrderBy(rnd.Next));
            //var datestack1 = new Stack<DateTimeOffset>(dates1.OrderBy(_ => rnd.Next()));
            //var datestack1 = new Stack<DateTimeOffset>(new List<DateTimeOffset>() { dates1[0], dates1[1], dates1[2] });
            var datestack1 = new Stack<DateTimeOffset>(dates1);
            var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

            var titles2 = Enumerable.Range(1, count).Select(x => ((char)('c' + x)).ToString()).ToList();
            var dates2 = Enumerable.Range(1, count).Select(x => now + TimeSpan.FromMinutes(x)).ToList();

            var idstack2 = new Stack<int>(Enumerable.Range(1, count).OrderBy(rnd.Next));
            //var datestack2 = new Stack<DateTimeOffset>(dates2.OrderBy(_ => rnd.Next()));
            //var datestack2 = new Stack<DateTimeOffset>(dates2);
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

            var list1 = Observable.Defer(() => Enumerable.Range(1, count)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack1.Pop();
                    var date = datestack1.Pop();
                    var title = titlestack1.Pop();

                    var s = Substitute.For<IPullRequestModel, INotifyPropertyChanged, IStringify>();
                    s.Title.Returns(title);
                    s.CreatedAt.Returns(date);
                    s.Number.Returns(id);
                    (s as IStringify).GetString().Returns(string.Format("{0} {1}", id, date.ToString()));
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == id);
                    s.When(c => (c as IPullRequestModel).CopyFrom(Arg.Any<IPullRequestModel>())).Do(c =>
                    {
                        var newtitle = c.Arg<IPullRequestModel>().Title;
                        var newdate = c.Arg<IPullRequestModel>().CreatedAt;
                        var id1 = c.Arg<IPullRequestModel>().Number;
                        s.Title.Returns(newtitle);
                        (s as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("Title"));
                        s.CreatedAt.Returns(newdate);
                        (s as IStringify).GetString().Returns(string.Format("{0} {1}", id1, newdate.ToString()));
                        System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "log.txt"),
                            string.Format("raising event {0} {1}\r\n", id1, newdate.ToString()));
                        (s as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("CreatedAt"));
                    });
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var list2 = Observable.Defer(() => Enumerable.Range(1, count)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack2.Pop();
                    var date = datestack2.Pop();
                    var title = titlestack2.Pop();

                    var s = Substitute.For<IPullRequestModel, INotifyPropertyChanged, IStringify>();
                    s.Title.Returns(title);
                    s.CreatedAt.Returns(date);
                    s.Number.Returns(id);
                    (s as IStringify).GetString().Returns(string.Format("{0} {1}", id, date.ToString()));
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == id);
                    s.When(c => (c as IPullRequestModel).CopyFrom(Arg.Any<IPullRequestModel>())).Do(c =>
                    {
                        var newtitle = c.Arg<IPullRequestModel>().Title;
                        var newdate = c.Arg<IPullRequestModel>().CreatedAt;
                        var id1 = c.Arg<IPullRequestModel>().Number;
                        s.Title.Returns(newtitle);
                        (s as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("Title"));
                        s.CreatedAt.Returns(newdate);
                        (s as IStringify).GetString().Returns(string.Format("{0} {1}", id1, newdate.ToString()));
                        System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "log.txt"),
                            string.Format("raising event {0} {1}\r\n", id1, newdate.ToString()));
                        (s as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("CreatedAt"));
                    });

                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var original = new TrackingCollection<IPullRequestModel>(list1.Concat(list2));
            var col = original.CreateDerivedCollection(x => x,
                orderer: OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.CreatedAt).Compare);
            original.Subscribe();


            Assert.Collection(col, dates2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.CreatedAt))).Reverse().ToArray());

            //col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            //Assert.Collection(col, Enumerable.Range(1, count).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).ToArray());

            //col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.CreatedAt).Compare);
            //Assert.Collection(col, dates2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.CreatedAt))).ToArray());

            //col.SetComparer(OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.Title).Compare);
            //Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).Reverse().ToArray());

            //col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Title).Compare);
            //Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).ToArray());
        }

        [Fact]
        public async void FilterAndSort()
        {
            var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
            var count = 20;
            var rnd = new Random();

            var titles1 = Enumerable.Range(1, count).Select(x => ((char)('a' + x)).ToString()).ToList();
            var dates1 = Enumerable.Range(1, count).Select(x => now + TimeSpan.FromMinutes(x)).ToList();

            var idstack1 = new Stack<int>(Enumerable.Range(1, count).OrderBy(rnd.Next));
            //var datestack1 = new Stack<DateTimeOffset>(dates1.OrderBy(_ => rnd.Next()));
            //var datestack1 = new Stack<DateTimeOffset>(new List<DateTimeOffset>() { dates1[0], dates1[1], dates1[2] });
            var datestack1 = new Stack<DateTimeOffset>(dates1);
            var titlestack1 = new Stack<string>(titles1.OrderBy(_ => rnd.Next()));

            var titles2 = Enumerable.Range(1, count).Select(x => ((char)('c' + x)).ToString()).ToList();
            var dates2 = Enumerable.Range(1, count).Select(x => now + TimeSpan.FromMinutes(x)).ToList();

            var idstack2 = new Stack<int>(Enumerable.Range(1, count).OrderBy(rnd.Next));
            //var datestack2 = new Stack<DateTimeOffset>(dates2.OrderBy(_ => rnd.Next()));
            //var datestack2 = new Stack<DateTimeOffset>(dates2);
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

            var list = new List<IPullRequestModel>(Enumerable.Range(1, count)
                .Select(i =>
                {
                    var id = idstack1.Pop();
                    var date = datestack1.Pop();
                    var title = titlestack1.Pop();
                    return new PullRequestModel(id, title, Substitute.For<IAccount>(), date, date);
                }));

            var cache = list.ToObservable();
            var live = Observable.Generate(
                1,
                i => i != count,
                i => i + 1,
                i =>
                {
                    var id = idstack2.Pop();
                    var date = datestack2.Pop();
                    var title = titlestack2.Pop();
                    return new PullRequestModel(id, title, Substitute.For<IAccount>(), date, date);
                },
                i => TimeSpan.FromMilliseconds(300)
            );

/*
            var list1 = Observable.Defer(() => Enumerable.Range(1, count)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack1.Pop();
                    var date = datestack1.Pop();
                    var title = titlestack1.Pop();

                    var s = Substitute.For<IPullRequestModel, INotifyPropertyChanged, IStringify>();
                    s.Title.Returns(title);
                    s.CreatedAt.Returns(date);
                    s.Number.Returns(id);
                    (s as IStringify).GetString().Returns(string.Format("{0} {1}", id, date.ToString()));
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == id);
                    s.When(c => (c as IPullRequestModel).CopyFrom(Arg.Any<IPullRequestModel>())).Do(c =>
                    {
                        var newtitle = c.Arg<IPullRequestModel>().Title;
                        var newdate = c.Arg<IPullRequestModel>().CreatedAt;
                        var id1 = c.Arg<IPullRequestModel>().Number;
                        s.Title.Returns(newtitle);
                        (s as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("Title"));
                        s.CreatedAt.Returns(newdate);
                        (s as IStringify).GetString().Returns(string.Format("{0} {1}", id1, newdate.ToString()));
                        (s as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("CreatedAt"));
                    });
                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();

            var list2 = Observable.Defer(() => Enumerable.Range(1, count)
                .OrderBy(rnd.Next)
                .Select(x =>
                {
                    var id = idstack2.Pop();
                    var date = datestack2.Pop();
                    var title = titlestack2.Pop();

                    var s = Substitute.For<IPullRequestModel, INotifyPropertyChanged, IStringify>();
                    s.Title.Returns(title);
                    s.CreatedAt.Returns(date);
                    s.Number.Returns(id);
                    (s as IStringify).GetString().Returns(string.Format("{0} {1}", id, date.ToString()));
                    s.Equals(Arg.Any<IPullRequestModel>()).Returns(c => c.Arg<IPullRequestModel>().Number == id);
                    s.When(c => (c as IPullRequestModel).CopyFrom(Arg.Any<IPullRequestModel>())).Do(c =>
                    {
                        var newtitle = c.Arg<IPullRequestModel>().Title;
                        var newdate = c.Arg<IPullRequestModel>().CreatedAt;
                        var id1 = c.Arg<IPullRequestModel>().Number;
                        s.Title.Returns(newtitle);
                        (s as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("Title"));
                        s.CreatedAt.Returns(newdate);
                        (s as IStringify).GetString().Returns(string.Format("{0} {1}", id1, newdate.ToString()));
                        System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "log.txt"),
                            string.Format("raising event {0} {1}\r\n", id1, newdate.ToString()));
                        (s as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("CreatedAt"));
                    });

                    return s;
                })
                .ToObservable())
                .Replay()
                .RefCount();
*/
            var result = cache.Merge(live);
            var col = new TrackingCollection<IPullRequestModel>(result,
                OrderedComparer<IPullRequestModel>.OrderBy(x => x.CreatedAt).Compare,
                (t, idx, lst) => lst.Count < 5);

            ManualResetEvent ev = new ManualResetEvent(false);
            col.Subscribe(_ => { }, () =>
            {
                ev.Set();
            });

            ev.WaitOne();

            System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "log.txt"),
                            string.Join("\r\n", col.Select(p => p.CreatedAt)));
            System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "log.txt"),
                "\r\n==\r\n");
            System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "log.txt"),
                            string.Join("\r\n", dates2));

            Assert.Equal(5, col.Count);

            //col.SetFilter(null);
            //System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "log.txt"),
            //    "\r\n==\r\n");
            //System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "log.txt"),
            //                string.Join("\r\n", col.Select(p => p.CreatedAt)));
            Assert.Collection(col, dates2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.CreatedAt))).Take(5).ToArray());

            //col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Number).Compare);
            //Assert.Collection(col, Enumerable.Range(1, count).Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Number))).ToArray());

            //col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.CreatedAt).Compare);
            //Assert.Collection(col, dates2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.CreatedAt))).ToArray());

            //col.SetComparer(OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.Title).Compare);
            //Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).Reverse().ToArray());

            //col.SetComparer(OrderedComparer<IPullRequestModel>.OrderBy(x => x.Title).Compare);
            //Assert.Collection(col, titles2.Select(x => new Action<IPullRequestModel>(t => Assert.Equal(x, t.Title))).ToArray());
        }
    }
}
