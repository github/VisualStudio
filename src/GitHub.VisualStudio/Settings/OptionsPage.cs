using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.InteropServices;
using GitHub.Exports;
using GitHub.Logging;
using GitHub.Settings;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio.Shell;
using Serilog;

namespace GitHub.VisualStudio
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("68C87C7B-0212-4256-BB6D-6A6BB847A3A7")]
    public class OptionsPage : UIElementDialogPage
    {
        static readonly ILogger log = LogManager.ForContext<OptionsPage>();

        OptionsControl child;
        IPackageSettings packageSettings;

        protected override UIElement Child
        {
            get
            {
                if (!ExportForVisualStudioProcessAttribute.IsVisualStudioProcess())
                {
                    return new Grid(); // Show blank page
                }

                return child ?? (child = new OptionsControl());
            }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            if (!ExportForVisualStudioProcessAttribute.IsVisualStudioProcess())
            {
                log.Warning("Don't activate options for non-Visual Studio process");
                return;
            }

            base.OnActivate(e);
            packageSettings = Services.DefaultExportProvider.GetExportedValue<IPackageSettings>();
            LoadSettings();
        }

        void LoadSettings()
        {
            child.CollectMetrics = packageSettings.CollectMetrics;
            child.EditorComments = packageSettings.EditorComments;
            child.EnableTraceLogging = packageSettings.EnableTraceLogging;
        }

        void SaveSettings()
        {
            packageSettings.CollectMetrics = child.CollectMetrics;
            packageSettings.EditorComments = child.EditorComments;
            packageSettings.EnableTraceLogging = child.EnableTraceLogging;
            packageSettings.Save();
        }

        protected override void OnApply(PageApplyEventArgs args)
        {
            if (!ExportForVisualStudioProcessAttribute.IsVisualStudioProcess())
            {
                log.Warning("Don't apply options for non-Visual Studio process");
                return;
            }

            if (args.ApplyBehavior == ApplyKind.Apply)
            {
                SaveSettings();
            }

            base.OnApply(args);
        }
    }
}
