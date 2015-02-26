using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.TeamFoundation.Client;
using NullGuard;

namespace GitHub.VisualStudio.Base
{
    public abstract class TeamExplorerBase : IDisposable, INotifyPropertyChanged
    {
        bool subscribed = false;
        bool disposed = false;

        IServiceProvider serviceProvider;
        protected IServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set
            {
                if (serviceProvider != null)
                    UnsubscribeContextChanges();
                serviceProvider = value;
                if (serviceProvider != null)
                    SubscribeContextChanges();
            }
        }

        protected ITeamFoundationContext CurrentContext
        {
            get
            {
                var manager = GetService<ITeamFoundationContextManager>();
                if (manager != null)
                    return manager.CurrentContext;
                return null;
            }
        }

        void SubscribeContextChanges()
        {
            Debug.Assert(serviceProvider != null, "ServiceProvider must be set before subscribing to context changes");
            if (serviceProvider == null || subscribed)
                return;

            var manager = GetService<ITeamFoundationContextManager>();
            if (manager != null)
            {
                manager.ContextChanged += ContextChanged;
                subscribed = true;
            }
        }

        void UnsubscribeContextChanges()
        {
            Debug.Assert(serviceProvider != null, "ServiceProvider must be set before subscribing to context changes");
            if (serviceProvider == null || !subscribed)
                return;

            var manager = GetService<ITeamFoundationContextManager>();
            if (manager != null)
            {
                manager.ContextChanged -= ContextChanged;
                subscribed = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TeamExplorerBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                UnsubscribeContextChanges();

            disposed = true;
        }

        [return: AllowNull]
        public T GetService<T>()
        {
            Debug.Assert(this.serviceProvider != null, "GetService<T> called before service provider is set");
            if (serviceProvider == null)
                return default(T);
            return (T)serviceProvider.GetService(typeof(T));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [return: AllowNull]
        public Ret GetService<T, Ret>() where Ret : class
        {
            return GetService<T>() as Ret;
        }


        protected virtual void ContextChanged(object sender, ContextChangedEventArgs e)
        {
        }

    }
}
