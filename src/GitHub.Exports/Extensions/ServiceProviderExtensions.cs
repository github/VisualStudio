using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;

namespace GitHub.Extensions
{
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Safe variant of GetService that doesn't throw exceptions if the service is
        /// not found.
        /// </summary>
        /// <returns>The service, or null if not found</returns>
        public static object GetServiceSafe(this IServiceProvider serviceProvider, Type type)
        {
            var ui = serviceProvider as IGitHubServiceProvider;
            if (ui == null)
            {
                try
                {
                    var ret = serviceProvider.GetService(type);
                    if (ret != null)
                        return ret;
                }
                catch { }
            }

            try
            {
                if (ui == null)
                {
                    ui = serviceProvider.GetService(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;
                }
                if (type.IsEquivalentTo(typeof(IGitHubServiceProvider)))
                    return ui;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                VisualStudio.VsOutputLogger.WriteLine("GetServiceSafe: Could not obtain instance of '{0}'", type);
            }
            return ui?.TryGetService(type);
        }

        /// <summary>
        /// Safe generic variant that calls <see cref="TryGetService(IServiceProvider, Type)"/>
        /// so it doesn't throw exceptions if the service is not found
        /// </summary>
        /// <returns>The service, or null if not found</returns>
        public static T GetServiceSafe<T>(this IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.GetServiceSafe(typeof(T)) as T;
        }

        public static void AddCommandHandler(this IServiceProvider provider,
            Guid guid,
            int cmdId,
            EventHandler eventHandler)
        {
            var mcs = provider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            Debug.Assert(mcs != null, "No IMenuCommandService? Something is wonky");
            if (mcs == null)
                return;
            var id = new CommandID(guid, cmdId);
            var item = new MenuCommand(eventHandler, id);
            mcs.AddCommand(item);
        }

        public static OleMenuCommand AddCommandHandler(this IServiceProvider provider,
            Guid guid,
            int cmdId,
            Func<bool> canEnable,
            Action execute,
            bool disable = false)
        {
            var mcs = provider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            Debug.Assert(mcs != null, "No IMenuCommandService? Something is wonky");
            if (mcs == null)
                return null;
            var id = new CommandID(guid, cmdId);
            var item = new OleMenuCommand(
                (s, e) => execute(),
                (s, e) => { },
                (s, e) =>
                {
                    if (disable)
                        ((OleMenuCommand)s).Enabled = canEnable();
                    else
                        ((OleMenuCommand)s).Visible = canEnable();
                },
                id);
            mcs.AddCommand(item);
            return item;
        }

        public static OleMenuCommand AddCommandHandler<TParam>(this IServiceProvider provider,
            Guid guid,
            int cmdId,
            Func<bool> canEnable,
            Action<TParam> execute,
            bool disable = false)
        {
            var mcs = provider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            Debug.Assert(mcs != null, "No IMenuCommandService? Something is wonky");
            if (mcs == null)
                return null;
            var id = new CommandID(guid, cmdId);
            var item = new OleMenuCommand(
                (s, e) => execute((TParam)((OleMenuCmdEventArgs)e).InValue),
                (s, e) => { },
                (s, e) =>
                {
                    if (disable)
                        ((OleMenuCommand)s).Enabled = canEnable();
                    else
                        ((OleMenuCommand)s).Visible = canEnable();
                },
                id);
            mcs.AddCommand(item);
            return item;
        }
    }
}
