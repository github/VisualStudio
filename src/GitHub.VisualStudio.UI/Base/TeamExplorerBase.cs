using System;
using System.ComponentModel;
using System.Diagnostics;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Base
{
    public abstract class TeamExplorerBase : NotificationAwareObject, IDisposable
    {
        public static readonly Guid TeamExplorerConnectionsSectionId = new Guid("ef6a7a99-f01f-4c91-ad31-183c1354dd97");

        protected IServiceProvider TEServiceProvider
        {
            get; set;
        }

        protected IGitHubServiceProvider ServiceProvider
        {
            get;
        }

        protected TeamExplorerBase(IGitHubServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            ServiceProvider = serviceProvider;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected static void OpenInBrowser(Lazy<IVisualStudioBrowser> browser, Uri uri)
        {
            Guard.ArgumentNotNull(browser, nameof(browser));
            Guard.ArgumentNotNull(uri, nameof(uri));

            OpenInBrowser(browser.Value, uri);
        }

        protected static void OpenInBrowser(IVisualStudioBrowser browser, Uri uri)
        {
            Guard.ArgumentNotNull(browser, nameof(browser));
            Guard.ArgumentNotNull(uri, nameof(uri));

            browser?.OpenUrl(uri);
        }
    }
}
