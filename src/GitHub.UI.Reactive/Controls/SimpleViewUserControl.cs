using System;
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
    /// <summary>
    /// Base class for all of our user controls. This one does not import GitHub resource/styles and is used by the 
    /// publish control.
    /// </summary>
    public class SimpleViewUserControl : UserControl, IDisposable, IActivatable
    {
        readonly Subject<object> close = new Subject<object>();
        readonly Subject<object> cancel = new Subject<object>();
        readonly Subject<bool> isBusy = new Subject<bool>();

        public SimpleViewUserControl()
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

        public IObservable<bool> IsBusy => isBusy;

        protected void NotifyDone()
        {
            close.OnNext(null);
            close.OnCompleted();
        }

        protected void NotifyCancel()
        {
            cancel.OnNext(null);
            cancel.OnCompleted();
        }

        protected void NotifyIsBusy(bool busy)
        {
            isBusy.OnNext(busy);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                close.Dispose();
                cancel.Dispose();
                isBusy.Dispose();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class SimpleViewUserControl<TViewModel, TImplementor> : SimpleViewUserControl, IViewFor<TViewModel>, IView 
        where TViewModel : class, IViewModel where TImplementor : class
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(TViewModel), typeof(TImplementor), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        public TViewModel ViewModel
        {
            [return: AllowNull]
            get { return (TViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        TViewModel IViewFor<TViewModel>.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value; }
        }
    }
}
