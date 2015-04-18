using System;
using System.Reactive.Subjects;
using System.Windows.Controls;

namespace GitHub.UI
{
    /// <summary>
    /// Base class for all of our user controls. This one does not import GitHub resource/styles and is used by the 
    /// publish control.
    /// </summary>
    public class SimpleViewUserControl : UserControl, IDisposable
    {
        readonly Subject<object> close = new Subject<object>();
        readonly Subject<object> cancel = new Subject<object>();

        public SimpleViewUserControl()
        {
        }

        public IObservable<object> Done { get { return close; } }

        public IObservable<object> Cancel { get { return cancel; } }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    close.Dispose();
                }
                disposed = true;
            }
        }

    }
}
