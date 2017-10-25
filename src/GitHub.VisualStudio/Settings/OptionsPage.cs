using GitHub.Services;
using GitHub.Settings;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace GitHub.VisualStudio
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("68C87C7B-0212-4256-BB6D-6A6BB847A3A7")]
    public class OptionsPage : UIElementDialogPage
    {
        OptionsControl child;
        IPackageSettings packageSettings;

        protected override UIElement Child
        {
            get { return child ?? (child = new OptionsControl()); }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            packageSettings = ServiceHelper.DefaultExportProvider.GetExportedValue<IPackageSettings>();
            LoadSettings();
        }

        void LoadSettings()
        {
            child.CollectMetrics = packageSettings.CollectMetrics;
            child.EditorComments = packageSettings.EditorComments;
        }

        void SaveSettings()
        {
            packageSettings.CollectMetrics = child.CollectMetrics;
            packageSettings.EditorComments = child.EditorComments;
            packageSettings.Save();
        }

        protected override void OnApply(PageApplyEventArgs args)
        {
            if (args.ApplyBehavior == ApplyKind.Apply)
            {
                SaveSettings();
            }

            base.OnApply(args);
        }
    }
}
