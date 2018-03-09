using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using NUnit.Framework;

namespace GitHub.UI.UnitTests.Controls
{
    [TestFixture, RequiresThread(ApartmentState.STA)]
    public class QuickListPresenterTests
    {
        static readonly Size Infinity = new Size(double.PositiveInfinity, double.PositiveInfinity);

        [Test]
        public void Measure_With_No_Items_Should_Not_Create_Container()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
            };

            target.Measure(Infinity);

            Assert.That(target.Children, Is.Empty);
        }

        [Test]
        public void Measure_Should_Create_Container_For_Measurement()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[10],
            };

            target.Measure(Infinity);

            Assert.That(target.Children.Count, Is.EqualTo(1));
        }

        [Test]
        public void Measure_Should_Return_Correct_DesiredSize_Infinity()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[10],
            };

            target.Measure(Infinity);

            Assert.That(target.DesiredSize, Is.EqualTo(new Size(100, 100)));
        }

        [Test]
        public void Measure_Should_Return_Correct_DesiredSize_Smaller()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 90));

            Assert.That(target.DesiredSize, Is.EqualTo(new Size(80, 90)));
        }

        [Test]
        public void Arrange_Should_Fill_Panel_With_Containers()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));

            Assert.That(target.Children.Count, Is.EqualTo(10));
        }

        [Test]
        public void Arrange_Should_Fill_Panel_With_Containers_Partial_Item()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            Assert.That(target.Children.Count, Is.EqualTo(10));
        }

        [Test]
        public void Viewport_Should_Be_Rounded_Up()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            Assert.That(target.ViewportHeight, Is.EqualTo(10));
        }

        [Test]
        public void Arrange_Should_Fill_Panel_With_Containers_With_Spacing()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
                Spacing = 1,
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            Assert.That(target.Children.Count, Is.EqualTo(9));
        }

        [Test]
        public void Arrange_Should_Not_Create_More_Containers_Than_Items()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[5],
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            Assert.That(target.Children.Count, Is.EqualTo(5));
        }

        [Test]
        public void Enlarging_Should_Add_Containers()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));
            target.InvalidateArrange();
            target.Measure(new Size(80, 105));
            target.Arrange(new Rect(0, 0, 80, 105));

            Assert.That(target.Children.Count, Is.EqualTo(11));
        }

        [Test]
        public void Shrinking_Should_Remove_Containers()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));
            target.InvalidateArrange();
            target.Measure(new Size(80, 90));
            target.Arrange(new Rect(0, 0, 80, 90));

            Assert.That(target.Children.Count, Is.EqualTo(9));
        }

        [Test]
        public void Arrange_Should_Assign_DataContexts()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 10).ToList(),
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));

            Assert.That(
                target.Children.Cast<FrameworkElement>().Select(x => x.DataContext),
                Is.EqualTo(Enumerable.Range(0, 10)));
        }

        [Test]
        public void SetVerticalOffset_Should_Update_Offset()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));
            target.SetVerticalOffset(10);

            Assert.That(target.VerticalOffset, Is.EqualTo(10));
        }

        [Test]
        public void SetVerticalOffset_Should_Truncate_Fractional_Offset()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));
            target.SetVerticalOffset(1.6);

            Assert.That(target.VerticalOffset, Is.EqualTo(1));
        }

        [Test]
        public void Partially_Visible_Item_Should_Add_1_To_Extent()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 10).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            Assert.That(target.ExtentHeight, Is.EqualTo(11));
        }

        [Test]
        public void SetVerticalOffset_Scroll_Past_End_To_Show_Partially_Visible_Item()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));
            target.SetVerticalOffset(11);

            Assert.That(target.VerticalOffset, Is.EqualTo(11));
        }

        [Test]
        public void SetVerticalOffset_Should_Coerce_Negative_Offset_To_0()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 10).ToList(),
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));
            target.SetVerticalOffset(-1);

            Assert.That(target.VerticalOffset, Is.EqualTo(0));
        }

        [Test]
        public void SetVerticalOffset_Should_Coerce_Too_Large_A_Value()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));
            target.SetVerticalOffset(11);

            Assert.That(target.VerticalOffset, Is.EqualTo(10));
        }

        [Test]
        public void SetVerticalOffset_Plus1_Should_Assign_DataContexts()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));
            target.SetVerticalOffset(1);

            Assert.That(
                target.Children.Cast<FrameworkElement>().Select(x => x.DataContext),
                Is.EqualTo(Enumerable.Range(1, 10)));
        }

        [Test]
        public void SetVerticalOffset_Plus1_Should_Move_First_Control_To_End()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));

            var first = target.Children[0];
            target.SetVerticalOffset(1);

            Assert.That(target.Children[9], Is.SameAs(first));
        }

        [Test]
        public void SetVerticalOffset_Plus1_Past_End_Should_Assign_DataContexts()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 10).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            var first = target.Children[0];
            target.SetVerticalOffset(1);

            Assert.That(
                target.Children.Cast<FrameworkElement>().Select(x => x.DataContext),
                Is.EqualTo(Enumerable.Range(1, 9).Concat(new[] { 0 })));
        }

        [Test]
        public void SetVerticalOffset_Plus1_Past_End_Should_Hide_Last_Control()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 10).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            var first = target.Children[0];
            target.SetVerticalOffset(1);

            Assert.That(target.Children[9].Visibility, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void SetVerticalOffset_Minus1_Should_Assign_DataContexts()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));
            target.SetVerticalOffset(2);
            target.SetVerticalOffset(1);

            Assert.That(
                target.Children.Cast<FrameworkElement>().Select(x => x.DataContext),
                Is.EqualTo(Enumerable.Range(1, 10)));
        }

        [Test]
        public void SetVerticalOffset_Minus1_Should_Move_Last_Control_To_Start()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));
            target.SetVerticalOffset(1);

            var last = target.Children[9];
            target.SetVerticalOffset(0);

            Assert.That(target.Children[0], Is.SameAs(last));
        }

        [Test]
        public void SetVerticalOffset_Minus_0_5_From_Past_End_Should_Show_Last_Control()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 10).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            var first = target.Children[0];
            target.SetVerticalOffset(1);
            target.SetVerticalOffset(0);

            Assert.That(target.Children[9].Visibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void SetVerticalOffset_10_Should_Assign_DataContexts()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));
            target.SetVerticalOffset(10);

            Assert.That(
                target.Children.Cast<FrameworkElement>().Select(x => x.DataContext),
                Is.EqualTo(Enumerable.Range(10, 10)));
        }

        [Test]
        public void SetVerticalOffset_11_Past_End_Should_Hide_Last_Control()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            var first = target.Children[0];
            target.SetVerticalOffset(11);

            Assert.That(target.Children[9].Visibility, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void SetVerticalOffset_11_Past_End_Should_Assign_DataContexts()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            var first = target.Children[0];
            target.SetVerticalOffset(11);

            Assert.That(
                target.Children.Cast<FrameworkElement>().Select(x => x.DataContext),
                Is.EqualTo(Enumerable.Range(11, 9).Concat(new[] { 9 })));
        }

        [Test]
        public void SetVerticalOffset_Minus_5_5_From_Past_End_Should_Show_Last_Control()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            var first = target.Children[0];
            target.SetVerticalOffset(10.5);
            target.SetVerticalOffset(5);

            Assert.That(target.Children[9].Visibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void Arrange_Should_Coerce_Offset()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new string[20],
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));
            target.SetVerticalOffset(10);
            target.Measure(new Size(80, 200));
            target.Arrange(new Rect(0, 0, 80, 200));

            Assert.That(((IScrollInfo)target).VerticalOffset, Is.EqualTo(0));
        }

        [Test]
        public void Item_Being_Removed_By_Indexer_Should_Invalidate_Measure()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = new AutoItemRemoveList(10, 5, 2),
            };

            target.Measure(new Size(80, 100));
            target.Arrange(new Rect(0, 0, 80, 100));

            Assert.False(target.IsMeasureValid);
        }

        [Test]
        public void Enlarging_With_Last_Item_Hidden_Should_Make_It_Visible()
        {
            var target = new QuickListPresenter
            {
                ItemContainerType = typeof(TestContainer),
                ItemsSource = Enumerable.Range(0, 20).ToList(),
            };

            target.Measure(new Size(80, 95));
            target.Arrange(new Rect(0, 0, 80, 95));

            var first = target.Children[0];
            target.SetVerticalOffset(11);
            target.Measure(new Size(80, 120));
            target.Arrange(new Rect(0, 0, 80, 120));

            Assert.That(target.Children[9].Visibility, Is.EqualTo(Visibility.Visible));
        }

        class TestContainer : FrameworkElement
        {
            public TestContainer()
            {
                Width = 100;
                Height = 10;
            }
        }

        class AutoItemRemoveList : IList, INotifyCollectionChanged
        {
            List<int> items;
            int whenAccessed;
            int removeAt;

            public AutoItemRemoveList(
                int count,
                int whenAccessed,
                int removeAt)
            {
                items = Enumerable.Range(0, count).ToList();
                this.whenAccessed = whenAccessed;
                this.removeAt = removeAt;
            }

            public object this[int index]
            {
                get
                {
                    if (index == whenAccessed)
                    {
                        items.RemoveAt(removeAt);
                        whenAccessed = -1;
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }

                    return items[index];
                }

                set { throw new NotImplementedException(); }
            }

            public int Count => items.Count;
            public bool IsFixedSize => false;
            public bool IsReadOnly => false;
            public bool IsSynchronized => false;
            public object SyncRoot => null;

            public int Add(object value) => 0;
            public void Clear() { }
            public bool Contains(object value) => false;
            public void CopyTo(Array array, int index) { }
            public IEnumerator GetEnumerator() => null;
            public int IndexOf(object value) => 0;
            public void Insert(int index, object value) { }
            public void Remove(object value) { }
            public void RemoveAt(int index) { }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
        }
    }
}
