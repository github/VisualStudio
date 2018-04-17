using GitHub.Settings;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using GitHub.Services;

namespace GitHub.VisualStudio
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("68C87C7B-0212-4256-BB6D-6A6BB847A3A7")]
    public class OptionsPage : UIElementDialogPage
    {
        OptionsControl child;
        IPackageSettings packageSettings;
        IUsageService usageService;

        protected override UIElement Child
        {
            get { return child ?? (child = new OptionsControl()); }
        }

        protected override async void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            packageSettings = (IPackageSettings) await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(IPackageSettings));
            usageService = (IUsageService) await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(IUsageService));
            LoadSettings();
        }

        void LoadSettings()
        {
            child.CollectMetrics = packageSettings.CollectMetrics;
            child.EditorComments = packageSettings.EditorComments;
            child.EnableTraceLogging = packageSettings.EnableTraceLogging;
        }

        async Task SaveSettings()
        {
            var metricsChanged = packageSettings.CollectMetrics != child.CollectMetrics;
            packageSettings.CollectMetrics = child.CollectMetrics;
            packageSettings.EditorComments = child.EditorComments;
            packageSettings.EnableTraceLogging = child.EnableTraceLogging;
            packageSettings.Save();
            var userData = await usageService.ReadUserData();
            userData.SentOptIn = !metricsChanged;
            await usageService.WriteUserData(userData);
        }

        protected override async void OnApply(PageApplyEventArgs args)
        {
            if (args.ApplyBehavior == ApplyKind.Apply)
            {
                await SaveSettings();
            }

            base.OnApply(args);
        }
    }
}
