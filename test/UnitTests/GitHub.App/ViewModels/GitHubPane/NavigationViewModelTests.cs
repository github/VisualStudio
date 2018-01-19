using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using NUnit.Framework;

public class NavigationViewModelTests
{
    public class TheContentProperty
    {
        [Test]
        public void ContentShouldInitiallyBeNull()
        {
            var target = new NavigationViewModel();

            Assert.That(target.Content, Is.Null);
        }

        [Test]
        public void ContentShouldBeSetOnNavigatingToPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            Assert.That(first, Is.EqualTo(target.Content));

            target.NavigateTo(second);
            Assert.That(second, Is.SameAs(target.Content));
        }

        [Test]
        public void ContentShouldBeSetOnNavigatingBack()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();

            Assert.That(first, Is.SameAs(target.Content));
        }

        [Test]
        public void ContentShouldBeSetOnNavigatingForward()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();
            target.Forward();

            Assert.That(second, Is.SameAs(target.Content));
        }

        [Test]
        public void ContentShouldBeSetWhenReplacingFuture()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();
            var third = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();
            target.NavigateTo(third);

            Assert.That(third, Is.EqualTo(target.Content));

            target.Back();

            Assert.That(first, Is.EqualTo(target.Content));
        }
    }

    public class TheForwardAndBackCommands
    {
        [Test]
        public void ForwardAndBackCommandsShouldInitiallyBeDisabled()
        {
            var target = new NavigationViewModel();

            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Test]
        public void ForwardAndBackCommandsShouldBeDisabledOnNavigatingToFirstPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();

            target.NavigateTo(first);

            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Test]
        public void BackCommandShouldBeEnabledOnNavigatingToSecondPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);

            Assert.True(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Test]
        public void ForwardCommandShouldBeEnabledOnNavigatingBack()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();

            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.True(target.NavigateForward.CanExecute(null));
        }

        [Test]
        public void BackShouldCallActivatedOnNewPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);

            first.ClearReceivedCalls();
            target.Back();

            first.Received(1).Activated();
        }

        [Test]
        public void BackShouldCallDeactivatedOnOldPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);

            second.ClearReceivedCalls();
            target.Back();

            second.Received(1).Deactivated();
        }

        [Test]
        public void ForwardShouldCallActivatedOnNewPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();

            second.ClearReceivedCalls();
            target.Forward();

            second.Received(1).Activated();
        }

        [Test]
        public void ForwardShouldCallDeactivatedOnOldPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();

            first.ClearReceivedCalls();
            target.Forward();

            first.Received(1).Deactivated();
        }
    }

    public class TheNavigateToMethod
    {
        [Test]
        public void ShouldCallActivatedOnNewPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();

            target.NavigateTo(first);

            first.Received(1).Activated();
        }

        [Test]
        public void ShouldCallDeactivatedOnOldPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            first.ClearReceivedCalls();
            target.NavigateTo(second);

            first.Received(1).Deactivated();
        }

        [Test]
        public void CloseRequestedShouldRemovePage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();
            var close = new Subject<Unit>();
            second.CloseRequested.Returns(close);

            target.NavigateTo(first);
            target.NavigateTo(second);
            close.OnNext(Unit.Default);

            //Assert.Single(target.History);
            Assert.That(first, Is.SameAs(target.History[0]));
        }

        [Test]
        public void NavigatingToExistingPageInForwardHistoryShouldNotDisposePage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();
            target.NavigateTo(second);

            second.DidNotReceive().Dispose();
        }
    }

    public class TheClearMethod
    {
        [Test]
        public void ClearsTheContentAndHistory()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Clear();

            Assert.That(target.Content, Is.Null);
            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Test]
        public void DisposesPages()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var disposed = false;

            first.When(x => x.Dispose()).Do(_ => disposed = true);

            target.NavigateTo(first);
            target.Clear();

            Assert.True(disposed);
        }

        [Test]
        public void CallsDeactivatedAndThenDisposedOnPages()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();

            target.NavigateTo(first);
            target.Clear();

            Received.InOrder(() =>
            {
                first.Deactivated();
                first.Dispose();
            });
        }
    }

    public class TheRemoveMethod
    {
        [Test]
        public void RemovesAllInstancesOfPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.NavigateTo(second);
            target.NavigateTo(second);
            target.RemoveAll(second);

            //Assert.Single(target.History);
        }

        [Test]
        public void RemovingItemAfterCurrentWorks()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();
            target.RemoveAll(second);

            Assert.That(first, Is.SameAs(target.Content));
            Assert.That(first, Is.SameAs(target.History[0]));
            Assert.That(0, Is.EqualTo(target.Index));
            //Assert.Single(target.History);
        }

        [Test]
        public void RemovingCurrentItemSetsContentToPrevious()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.RemoveAll(second);

            Assert.That(first, Is.SameAs(target.Content));
            Assert.That(first, Is.SameAs(target.History[0]));
            Assert.That(0, Is.EqualTo(target.Index));
            //Assert.Single(target.History);
        }

        [Test]
        public void RemovingOnlyItemWorks()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();

            target.NavigateTo(first);
            target.RemoveAll(first);

            Assert.That(target.Content, Is.Null);
            Assert.That(target.History, Is.Empty);
            Assert.That(-1, Is.EqualTo(target.Index));
        }

        [Test]
        public void RemovingItemCallsDispose()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var disposed = false;

            first.When(x => x.Dispose()).Do(_ => disposed = true);

            target.NavigateTo(first);
            target.RemoveAll(first);

            Assert.True(disposed);
        }

        [Test]
        public void CallsDeactivatedAndThenDisposedOnPages()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();

            target.NavigateTo(first);
            target.RemoveAll(first);

            Received.InOrder(() =>
            {
                first.Deactivated();
                first.Dispose();
            });
        }
    }

    static IPanePageViewModel CreatePage()
    {
        var result = Substitute.For<IPanePageViewModel>();
        result.CloseRequested.Returns(Observable.Never<Unit>());
        return result;
    }
}
