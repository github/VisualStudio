using System;
using System.ComponentModel.Composition;
using System.IO;
using GitHub.Services;
using Rothko;
using NLog;

namespace GitHub
{
    [Export(typeof(IVisualStudioBrowser))]
    public class Browser : IVisualStudioBrowser
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IProcessStarter processManager;
        readonly IEnvironment environment;

        [ImportingConstructor]
        public Browser(IProcessStarter processManager, IEnvironment environment)
        {
            this.processManager = processManager;
            this.environment = environment;
        }

        public void OpenUrl(Uri url)
        {
            if (url == null)
            {
                log.Warn("Attempted to open a null URL");
                return;
            }

            try
            {
                processManager.Start(url.ToString(), string.Empty);
                return;
            }
            catch (Exception ex)
            {
                log.Warn("Opening URL in default browser failed", ex);
            }

            try
            {
                processManager.Start(
                    Path.Combine(environment.GetProgramFilesPath(), @"Internet Explorer", "iexplore.exe"),
                    url.ToString());
            }
            catch (Exception ex)
            {
                log.Error("Really can't open the URL, even in IE", ex);
            }
        }
    }
}
