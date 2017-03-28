using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;

namespace GitHub.UI
{
    public class BusyStateView : ContentControl, IDisposable, IActivatable
    {
        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(BusyStateView));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(BusyStateView));

        readonly Subject<object> close = new Subject<object>();
        readonly Subject<object> cancel = new Subject<object>();

        static BusyStateView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BusyStateView),
                new FrameworkPropertyMetadata(typeof(BusyStateView)));
        }

        public BusyStateView()
        {
            this.WhenActivated(d =>
            {
                d(this.Events()
                    .KeyUp
                    .Where(x => x.Key == Key.Escape && !x.Handled)
                    .Subscribe(key =>
                    {
                        key.Handled = true;
                        NotifyCancel();
                    }));
            });
        }

        public IObservable<object> Done => close;

        public IObservable<object> Cancel => cancel;

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        protected void NotifyDone()
        {
            if (disposed)
                return;

            close.OnNext(null);
            close.OnCompleted();
        }

        protected void NotifyCancel()
        {
            if (disposed)
                return;

            cancel.OnNext(null);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                close.Dispose();
                cancel.Dispose();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class BusyStateView<TInterface, TImplementor> : BusyStateView, IViewFor<TInterface>, IView
        where TInterface : class, IViewModel
        where TImplementor : class
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(TInterface), typeof(TImplementor), new PropertyMetadata(null));

        IDisposable subscriptions;

        public BusyStateView()
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

                subscriptions = subs;
            };
        }

        [AllowNull]
        object IViewFor.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
            set { ViewModel = (TInterface)value; }
        }

        [AllowNull]
        IViewModel IView.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
        }

        [AllowNull]
        public TInterface ViewModel
        {
            [return: AllowNull]
            get { return (TInterface)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        [AllowNull]
        TInterface IViewFor<TInterface>.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
            set { ViewModel = value; }
        }
    }

}
