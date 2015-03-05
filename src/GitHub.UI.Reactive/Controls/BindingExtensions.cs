using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using GitHub.UI;
using ReactiveUI;

namespace GitHub.Controls
{
    public static class BindingExtensions
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "view")]
        public static IDisposable BindPassword<TViewModel, TView, TVMProp, TVProp>(this TView view,
            TViewModel viewModel,
            Expression<Func<TViewModel, TVMProp>> vmProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            SecurePasswordBox passwordBox) where TViewModel : class where TView : IViewFor
        {
            var oneWayBind = view.OneWayBind(viewModel, vmProperty, viewProperty);
            var bindTo = passwordBox.Events().TextChanged
                .Select(_ => passwordBox.Text)
                .BindTo(viewModel, vmProperty);
            return new CompositeDisposable(oneWayBind, bindTo);
        }
    }
}
