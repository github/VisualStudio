using System;
using System.Reactive.Subjects;
using System.Windows.Controls;
using GitHub.UI.Helpers;

namespace GitHub.UI
{
    public class ViewUserControl : UserControl, IDisposable
    {
        readonly Subject<object> close = new Subject<object>();

        public ViewUserControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
        }

        protected void NotifyDone()
        {
            close.OnNext(null);
            close.OnCompleted();
        }

        public IObservable<object> Done { get { return close; } }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
