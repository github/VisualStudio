using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace GitHub.Extensions
{
    public static class ReactiveUIExtensions
    {
        static ReactiveUIExtensions()
        {
            System.Diagnostics.Debugger.Break();
        }

        /// <summary>
        /// Syntactic sugar/convenience overload for the real RxUI onewaybind allowing one to specify
        /// conversion hint as a BooleanVisibilityHint without specifying it as a named parameter.
        /// </summary>
        public static IDisposable OneWayBind<TViewModel, TView, TVMProp, TVProp>(
            this TView view, 
            TViewModel viewModel, 
            Expression<Func<TViewModel, TVMProp>> vmProperty, 
            Expression<Func<TView, TVProp>> viewProperty, 
            BooleanToVisibilityHint conversionHint)
            where TViewModel : class
            where TView : IViewFor
        {
            return BindingMixins.OneWayBind(view, viewModel, vmProperty, viewProperty, conversionHint: conversionHint);
        }

        public static void WhenActivated(this ISupportsActivation This, params Func<IEnumerable<IDisposable>>[] blocks)
        {
            ViewForMixins.WhenActivated(This, () => blocks.SelectMany(x => x()));
        }

        public static bool TryExecute(this ICommand command, object parameter = null)
        {
            if (!command.CanExecute(parameter))
            {
                return false;
            }

            command.Execute(parameter);
            return true;
        }
    }
}
