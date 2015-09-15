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
        internal static readonly Guid TeamExplorerConnectionsSectionId = new Guid("ef6a7a99-f01f-4c91-ad31-183c1354dd97");

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

        protected static void OpenInBrowser(Lazy<IVisualStudioBrowser> browser, Uri uri)
        {
            OpenInBrowser(browser.Value, uri);
        }

        protected static void OpenInBrowser(IVisualStudioBrowser browser, Uri uri)
        {
            Debug.Assert(browser != null, "Could not create a browser helper instance.");
            browser?.OpenUrl(uri);
        }
    }
}
