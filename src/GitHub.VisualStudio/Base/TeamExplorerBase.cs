using System;
using System.ComponentModel;
using System.Diagnostics;
using GitHub.Primitives;
using NullGuard;
using GitHub.Services;

namespace GitHub.VisualStudio.Base
{
    public abstract class TeamExplorerBase : NotificationAwareObject, IDisposable
    {
        [AllowNull]
        protected IServiceProvider ServiceProvider
        {
            [return: AllowNull]
            get; set;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        [return: AllowNull]
        public T GetService<T>()
        {
            Debug.Assert(ServiceProvider != null, "GetService<T> called before service provider is set");
            if (ServiceProvider == null)
                return default(T);
            return (T)ServiceProvider.GetService(typeof(T));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [return: AllowNull]
        public Ret GetService<T, Ret>() where Ret : class
        {
            return GetService<T>() as Ret;
        }

        protected virtual void OpenInBrowser(Lazy<IVisualStudioBrowser> browser, Uri uri)
        {
            var b = browser.Value;
            Debug.Assert(b != null, "Could not create a browser helper instance.");
            if (b == null)
                return;
            b.OpenUrl(uri);
        }
    }
}
