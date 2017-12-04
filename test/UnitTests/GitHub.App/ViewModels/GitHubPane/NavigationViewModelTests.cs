using System;
using System.Reactive;
using System.Reactive.Subjects;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using Xunit;

public class NavigationViewModelTests
{
    public class TheContentProperty
    {
        [Fact]
        public void ContentShouldInitiallyBeNull()
        {
            var target = new NavigationViewModel();

            Assert.Null(target.Content);
        }

        [Fact]
        public void ContentShouldBeSetOnNavigatingToPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            Assert.Equal(first, target.Content);

            target.NavigateTo(second);
            Assert.Same(second, target.Content);
        }

        [Fact]
        public void ContentShouldBeSetOnNavigatingBack()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();

            Assert.Same(first, target.Content);
        }

        [Fact]
        public void ContentShouldBeSetOnNavigatingForward()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();
            target.Forward();

            Assert.Same(second, target.Content);
        }

        [Fact]
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

            Assert.Equal(third, target.Content);

            target.Back();

            Assert.Equal(first, target.Content);
        }
    }

    public class TheForwardAndBackCommands
    {
        [Fact]
        public void ForwardAndBackCommandsShouldInitiallyBeDisabled()
        {
            var target = new NavigationViewModel();

            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Fact]
        public void ForwardAndBackCommandsShouldBeDisabledOnNavigatingToFirstPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();

            target.NavigateTo(first);

            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
        [Fact]
        public void ShouldCallActivatedOnNewPage()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();

            target.NavigateTo(first);

            first.Received(1).Activated();
        }

        [Fact]
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

        [Fact]
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

            Assert.Single(target.History);
            Assert.Same(first, target.History[0]);
        }

        [Fact]
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
        [Fact]
        public void ClearsTheContentAndHistory()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Clear();

            Assert.Null(target.Content);
            Assert.False(target.NavigateBack.CanExecute(null));
            Assert.False(target.NavigateForward.CanExecute(null));
        }

        [Fact]
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

        [Fact]
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
        [Fact]
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

            Assert.Single(target.History);
        }

        [Fact]
        public void RemovingItemAfterCurrentWorks()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.Back();
            target.RemoveAll(second);

            Assert.Same(first, target.Content);
            Assert.Same(first, target.History[0]);
            Assert.Equal(0, target.Index);
            Assert.Single(target.History);
        }

        [Fact]
        public void RemovingCurrentItemSetsContentToPrevious()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var second = CreatePage();

            target.NavigateTo(first);
            target.NavigateTo(second);
            target.RemoveAll(second);

            Assert.Same(first, target.Content);
            Assert.Same(first, target.History[0]);
            Assert.Equal(0, target.Index);
            Assert.Single(target.History);
        }

        [Fact]
        public void RemovingOnlyItemWorks()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();

            target.NavigateTo(first);
            target.RemoveAll(first);

            Assert.Null(target.Content);
            Assert.Empty(target.History);
            Assert.Equal(-1, target.Index);
        }

        [Fact]
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

        [Fact]
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
        result.CloseRequested.Returns((IObservable<Unit>)null);
        return result;
    }
}
