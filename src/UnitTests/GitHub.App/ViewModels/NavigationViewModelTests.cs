using System;
using GitHub.ViewModels;
using Xunit;

namespace UnitTests.GitHub.App.ViewModels
{
    public class NavigationViewModelTests
    {
        public class TheContentProperty
        {
            [Fact]
            public void ContentShouldInitiallyBeNull()
            {
                var target = new NavigationViewModel<string>();

                Assert.Null(target.Content);
            }

            [Fact]
            public void ContentShouldBeSetOnNavigatingToPage()
            {
                var target = new NavigationViewModel<string>();

                target.NavigateTo("first");
                Assert.Equal("first", target.Content);

                target.NavigateTo("second");
                Assert.Equal("second", target.Content);
            }

            [Fact]
            public void ContentShouldBeSetOnNavigatingBack()
            {
                var target = new NavigationViewModel<string>();

                target.NavigateTo("first");
                target.NavigateTo("second");
                target.Back();

                Assert.Equal("first", target.Content);
            }

            [Fact]
            public void ContentShouldBeSetOnNavigatingForward()
            {
                var target = new NavigationViewModel<string>();

                target.NavigateTo("first");
                target.NavigateTo("second");
                target.Back();
                target.Forward();

                Assert.Equal("second", target.Content);
            }

            [Fact]
            public void ContentShouldBeSetWhenReplacingFuture()
            {
                var target = new NavigationViewModel<string>();

                target.NavigateTo("first");
                target.NavigateTo("second");
                target.Back();
                target.NavigateTo("third");

                Assert.Equal("third", target.Content);

                target.Back();

                Assert.Equal("first", target.Content);
            }
        }

        public class TheForwardAndBackCommands
        {
            [Fact]
            public void ForwardAndBackCommandsShouldInitiallyBeDisabled()
            {
                var target = new NavigationViewModel<string>();

                Assert.False(target.NavigateBack.CanExecute(null));
                Assert.False(target.NavigateForward.CanExecute(null));
            }

            [Fact]
            public void ForwardAndBackCommandsShouldBeDisabledOnNavigatingToFirstPage()
            {
                var target = new NavigationViewModel<string>();

                target.NavigateTo("first");

                Assert.False(target.NavigateBack.CanExecute(null));
                Assert.False(target.NavigateForward.CanExecute(null));
            }

            [Fact]
            public void BackCommandShouldBeEnabledOnNavigatingToSecondPage()
            {
                var target = new NavigationViewModel<string>();

                target.NavigateTo("first");
                target.NavigateTo("second");

                Assert.True(target.NavigateBack.CanExecute(null));
                Assert.False(target.NavigateForward.CanExecute(null));
            }

            [Fact]
            public void ForwardCommandShouldBeEnabledOnNavigatingBack()
            {
                var target = new NavigationViewModel<string>();

                target.NavigateTo("first");
                target.NavigateTo("second");
                target.Back();

                Assert.False(target.NavigateBack.CanExecute(null));
                Assert.True(target.NavigateForward.CanExecute(null));
            }
        }

        public class TheClearMethod
        {
            [Fact]
            public void ClearClearsTheContentAndHistory()
            {
                var target = new NavigationViewModel<string>();

                target.NavigateTo("first");
                target.NavigateTo("second");
                target.Clear();

                Assert.Null(target.Content);
                Assert.False(target.NavigateBack.CanExecute(null));
                Assert.False(target.NavigateForward.CanExecute(null));
            }
        }
    }
}
