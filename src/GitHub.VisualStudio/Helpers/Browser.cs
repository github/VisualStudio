using System;
using System.ComponentModel.Composition;
using System.IO;
using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Rothko;

namespace GitHub.VisualStudio.Helpers
{
    [Export(typeof(IVisualStudioBrowser))]
    public class Browser : IVisualStudioBrowser
    {
        readonly IServiceProvider serviceProvider;
        readonly IProcessStarter processManager;
        readonly IEnvironment environment;

        [ImportingConstructor]
        public Browser(IProcessStarter processManager, IEnvironment environment,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.processManager = processManager;
            this.environment = environment;
            this.serviceProvider = serviceProvider;
        }

        public void OpenUrl(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                return;

            /* First try to use the Web Browsing Service. This is not known to work because the
             * CreateExternalWebBrowser method always returns E_NOTIMPL. However, it is presumably
             * safer than a Shell Execute for arbitrary URIs.
             */
            var service = serviceProvider.GetWebBrowsingService();

            if (service != null)
            {
                __VSCREATEWEBBROWSER createFlags = __VSCREATEWEBBROWSER.VSCWB_AutoShow;
                VSPREVIEWRESOLUTION resolution = VSPREVIEWRESOLUTION.PR_Default;
                int result = ErrorHandler.CallWithCOMConvention(() =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    return service.CreateExternalWebBrowser((uint)createFlags, resolution, uri.AbsoluteUri);
                });

                if (ErrorHandler.Succeeded(result))
                    return;
            }

            // Fall back to Shell Execute, but only for http or https URIs
            if (uri.Scheme != "http" && uri.Scheme != "https")
                return;

            try
            {
                processManager.Start(uri.ToString(), string.Empty);
                return;
            }
            catch /*(Exception ex)*/
            {
                //log.Warn("Opening URL in default browser failed", ex);
            }

            try
            {
                processManager.Start(
                    Path.Combine(environment.GetProgramFilesPath(), @"Internet Explorer", "iexplore.exe"),
                    uri.ToString());
            }
            catch /*(Exception ex)*/
            {
                //log.Error("Really can't open the URL, even in IE", ex);
            }
        }
    }
}
