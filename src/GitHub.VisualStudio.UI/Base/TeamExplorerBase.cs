using System;
using System.ComponentModel;
using System.Diagnostics;
using GitHub.Primitives;
using NullGuard;
using GitHub.Services;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Base
{
    public abstract class TeamExplorerBase : NotificationAwareObject, IDisposable
    {
        public static readonly Guid TeamExplorerConnectionsSectionId = new Guid("ef6a7a99-f01f-4c91-ad31-183c1354dd97");

        [AllowNull]
        protected IServiceProvider TEServiceProvider
        {
            [return: AllowNull]
            get; set;
        }

        protected IGitHubServiceProvider ServiceProvider
        {
            [return: AllowNull]
            get;
        }

        protected TeamExplorerBase(IGitHubServiceProvider serviceProvider)
        {
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
            OpenInBrowser(browser.Value, uri);
        }

        protected static void OpenInBrowser(IVisualStudioBrowser browser, Uri uri)
        {
            Debug.Assert(browser != null, "Could not create a browser helper instance.");
            browser?.OpenUrl(uri);
        }
    }
}
