using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.UI;
using ReactiveUI;
using System.ComponentModel;
using GitHub.Extensions.Reactive;

namespace GitHub.Extensions
{
    public static class FocusHelper
    {
        static FocusHelper()
        {
            System.Diagnostics.Debugger.Break();
        }

        /// <summary>
        /// Attempts to move focus to an element within the provided container waiting for the element to be loaded
        /// if necessary (waits max 1 second to protect against confusing focus shifts if the element gets loaded much
        /// later).
        /// </summary>
        public static IObservable<bool> TryMoveFocus(this FrameworkElement element, FocusNavigationDirection direction)
        {
            return TryFocusImpl(element, e => e.MoveFocus(new TraversalRequest(direction)));
        }

        /// <summary>
        /// Attempts to move focus to the element, waiting for the element to be loaded
        /// if necessary (waits max 1 second to protect against confusing focus shifts 
        /// if the element gets loaded much later).
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "There's a Take(1) in there it'll be fine")]
        public static IObservable<bool> TryFocus(this FrameworkElement element)
        {
            return TryFocusImpl(element, e => e.Focus());
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "There's a Take(1) in there it'll be fine")]
        static IObservable<bool> TryFocusImpl(FrameworkElement element, Func<FrameworkElement, bool> focusAction)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return Observable.Return(false);
            }

            var elementObs = element.IsLoaded
                ? Observable.Return(element)
                : element.Events().Loaded
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                    .Select(_ => element);

            return elementObs
                .SelectMany(e =>
                {
                    var focusObs = Observable.Defer(() =>
                    {
                        if (focusAction(element))
                            return Observable.Return(true);

                        return Observable.Throw<bool>(new InvalidOperationException("Could not move focus"));
                    });

                    // MoveFocus almost always requires its descendant elements to be fully loaded, we
                    // have no way of knowing if they are so we'll try a few times before bailing out.
                    return focusObs.RetryWithBackoffStrategy(
                        retryCount: 5,
                        strategy: i => TimeSpan.FromMilliseconds(i * 50),
                        scheduler: RxApp.MainThreadScheduler
                    );
                })
                .Catch(Observable.Return(false))
                .FirstAsync();
        }
    }
}
