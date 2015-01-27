using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio.Base
{
    public abstract class TeamExplorerBase : IDisposable, INotifyPropertyChanged
    {
        bool subscribed = false;

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
            var manager = GetService<ITeamFoundationContextManager>();
            if (manager != null)
            {
                manager.ContextChanged -= ContextChanged;
                subscribed = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Dispose()
        {
            UnsubscribeContextChanges();
        }

        public T GetService<T>()
        {
            Debug.Assert(this.serviceProvider != null, "GetService<T> called before service provider is set");
            if (serviceProvider == null)
                return default(T);
            return (T)serviceProvider.GetService(typeof(T));
        }

        public Ret GetService<T, Ret>() where Ret : class
        {
            return GetService<T>() as Ret;
        }


        protected virtual void ContextChanged(object sender, ContextChangedEventArgs e)
        {
        }
    }
}
