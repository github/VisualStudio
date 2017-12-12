using System;
using System.Reactive;
using System.Reactive.Disposables;
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
    }

    public class TheNavigateToMethod
    {
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
        public void DisposesRegisteredResources()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var disposed = false;

            target.NavigateTo(first);
            target.RegisterDispose(first, Disposable.Create(() => disposed = true));
            target.Clear();

            Assert.True(disposed);
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
        public void RemovingItemDisposesRegisteredResources()
        {
            var target = new NavigationViewModel();
            var first = CreatePage();
            var disposed = false;

            target.NavigateTo(first);
            target.RegisterDispose(first, Disposable.Create(() => disposed = true));
            target.RemoveAll(first);

            Assert.True(disposed);
        }
    }

    static IPanePageViewModel CreatePage()
    {
        var result = Substitute.For<IPanePageViewModel>();
        result.CloseRequested.Returns((IObservable<Unit>)null);
        return result;
    }
}
