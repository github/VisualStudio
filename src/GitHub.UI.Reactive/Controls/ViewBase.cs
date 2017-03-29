using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;

namespace GitHub.UI
{
    /// <summary>
    /// Internal base class for views. Do not use, instead use <see cref="ViewBase{TInterface, TImplementor}"/>.
    /// </summary>
    /// <remarks>
    /// It is not possible in WPF XAML to create control templates for generic classes, so this class
    /// defines the dependency properties needed by <see cref="ViewBase{TInterface, TImplementor}"/>
    /// control template. The constructor is internal so this class cannot be inherited directly.
    /// </remarks>
    public class ViewBase : ContentControl
    {
        static readonly DependencyPropertyKey HasBusyStatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasBusyState),
                typeof(bool),
                typeof(ViewBase),
                new FrameworkPropertyMetadata());

        static readonly DependencyPropertyKey IsBusyPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsBusy),
                typeof(bool),
                typeof(ViewBase),
                new FrameworkPropertyMetadata());

        static readonly DependencyPropertyKey IsLoadingPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsLoading),
                typeof(bool),
                typeof(ViewBase),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty HasBusyStateProperty = HasBusyStatePropertyKey.DependencyProperty;
        public static readonly DependencyProperty IsBusyProperty = IsBusyPropertyKey.DependencyProperty;
        public static readonly DependencyProperty IsLoadingProperty = IsLoadingPropertyKey.DependencyProperty;

        static ViewBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ViewBase), new FrameworkPropertyMetadata(typeof(ViewBase)));
            FocusableProperty.OverrideMetadata(typeof(ViewBase), new FrameworkPropertyMetadata(false));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(ViewBase), new FrameworkPropertyMetadata(false));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(ViewBase), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(ViewBase), new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
        }

        /// <summary>
        /// Gets a value indicating whether the associated view model implements <see cref="IHasLoading"/>
        /// or <see cref="IHasBusy"/>.
        /// </summary>
        public bool HasBusyState
        {
            get { return (bool)GetValue(HasBusyStateProperty); }
            protected set { SetValue(HasBusyStatePropertyKey, value); }
        }

        /// <summary>
        /// Gets a value reflecting the associated view model's <see cref="IHasBusy.IsBusy"/> property.
        /// </summary>
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            protected set { SetValue(IsBusyPropertyKey, value); }
        }

        /// <summary>
        /// Gets a value reflecting the associated view model's <see cref="IHasLoading.IsLoading"/> property.
        /// </summary>
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            protected set { SetValue(IsLoadingPropertyKey, value); }
        }

        internal ViewBase()
        {
        }
    }

    /// <summary>
    /// Base class for views.
    /// </summary>
    /// <remarks>
    /// Exposes a typed <see cref="ViewModel"/> property and optionally displays <see cref="IHasLoading.IsLoading"/> 
    /// and <see cref="IHasBusy.IsBusy"/> state if the view model implements those interfaces. In addition, if the view
    /// model is an <see cref="IDialogViewModel"/>, invokes <see cref="IDialogViewModel.Cancel"/> when the escape key
    /// is pressed.
    /// </remarks>
    public class ViewBase<TInterface, TImplementor> : ViewBase, IView, IViewFor<TInterface>, IDisposable
        where TInterface : class, IViewModel
        where TImplementor : class
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(TInterface), typeof(TImplementor), new PropertyMetadata(null));

        IDisposable subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewBase{TInterface, TImplementor}"/> class.
        /// </summary>
        public ViewBase()
        {
            DataContextChanged += (s, e) =>
            {
                subscriptions?.Dispose();
                ViewModel = (TInterface)e.NewValue;

                var hasLoading = ViewModel as IHasLoading;
                var hasBusy = ViewModel as IHasBusy;
                var subs = new CompositeDisposable();

                if (hasLoading != null)
                {
                    subs.Add(this.OneWayBind(hasLoading, x => x.IsLoading, x => x.IsLoading));
                }

                if (hasBusy != null)
                {
                    subs.Add(this.OneWayBind(hasBusy, x => x.IsBusy, x => x.IsBusy));
                }

                HasBusyState = hasLoading != null || hasBusy != null;
                subscriptions = subs;
            };

            this.WhenActivated(d =>
            {
                d(this.Events()
                    .KeyUp
                    .Where(x => x.Key == Key.Escape && !x.Handled)
                    .Subscribe(key =>
                    {
                        key.Handled = true;
                        (this.ViewModel as IDialogViewModel)?.Cancel.Execute(null);
                    }));
            });
        }

        /// <summary>
        /// The control finalizer.
        /// </summary>
        ~ViewBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets the control's data context as a typed view model.
        /// </summary>
        [AllowNull]
        public TInterface ViewModel
        {
            [return: AllowNull]
            get { return (TInterface)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the control's data context as a typed view model. Required for interaction
        /// with ReactiveUI.
        /// </summary>
        [AllowNull]
        TInterface IViewFor<TInterface>.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
            set { ViewModel = value; }
        }

        /// <summary>
        /// Gets or sets the control's data context. Required for interaction with ReactiveUI.
        /// </summary>
        [AllowNull]
        object IViewFor.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
            set { ViewModel = (TInterface)value; }
        }

        /// <summary>
        /// Gets or sets the control's data context. Required for interaction with ReactiveUI.
        /// </summary>
        [AllowNull]
        IViewModel IView.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
        }

        /// <summary>
        /// Releases the managed resources held by the control.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the managed or unmanaged resources held by the control.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            subscriptions?.Dispose();
            subscriptions = null;
        }
    }
}
