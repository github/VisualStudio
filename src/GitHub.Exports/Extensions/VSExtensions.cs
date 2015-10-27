using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.Extensions
{
    public static class IServiceProviderExtensions
    {
        static IUIProvider cachedUIProvider = null;

        public static T TryGetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.TryGetService(typeof(T)) as T;
        }

        public static object TryGetService(this IServiceProvider serviceProvider, Type type)
        {
            if (cachedUIProvider != null && type == typeof(IUIProvider))
                return cachedUIProvider;

            var ui = serviceProvider as IUIProvider;
            if (ui != null)
                return ui.TryGetService(type);
            else
            {
                try
                {
                    return GetServiceAndCache(serviceProvider, type, ref cachedUIProvider);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
                return null;
            }
        }

        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            if (cachedUIProvider != null && typeof(T) == typeof(IUIProvider))
                return (T)cachedUIProvider;

            return (T)GetServiceAndCache(serviceProvider, typeof(T), ref cachedUIProvider);
        }

        public static T GetExportedValue<T>(this IServiceProvider serviceProvider)
        {
            if (cachedUIProvider != null && typeof(T) == typeof(IUIProvider))
                return (T)cachedUIProvider;

            var ui = serviceProvider as IUIProvider;
            return ui != null
                ? ui.GetService<T>()
                : GetExportedValueAndCache<T, IUIProvider>(ref cachedUIProvider);
        }

        public static ITeamExplorerSection GetSection(this IServiceProvider serviceProvider, Guid section)
        {
            return serviceProvider?.GetService<ITeamExplorerPage>()?.GetSection(section);
        }

        static object GetServiceAndCache<CacheType>(IServiceProvider provider, Type type, ref CacheType cache)
        {
            var ret = provider.GetService(type);
            if (type == typeof(CacheType))
                cache = (CacheType)ret;
            return ret;
        }

        static T GetExportedValueAndCache<T, CacheType>(ref CacheType cache)
        {
            var ret = VisualStudio.Services.ComponentModel.DefaultExportProvider.GetExportedValue<T>();
            if (typeof(T) == typeof(CacheType))
                cache = (CacheType)(object)ret;
            return ret;
        }

        public static void AddTopLevelMenuItem(this IServiceProvider provider,
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

        public static void AddDynamicMenuItem(this IServiceProvider provider,
            Guid guid,
            int cmdId,
            Func<bool> canEnable,
            Action execute)
        {
            var mcs = provider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            Debug.Assert(mcs != null, "No IMenuCommandService? Something is wonky");
            if (mcs == null)
                return;
            var id = new CommandID(guid, cmdId);
            var item = new OleMenuCommand(
                (s, e) => execute(),
                (s, e) => { },
                (s, e) =>
                {
                    ((OleMenuCommand)s).Visible = canEnable();
                },
                id);
            mcs.AddCommand(item);
        }
    }

    public static class ISolutionExtensions
    {
        public static IRepository GetRepoFromSolution(this IVsSolution solution)
        {
            string solutionDir, solutionFile, userFile;
            if (!ErrorHandler.Succeeded(solution.GetSolutionInfo(out solutionDir, out solutionFile, out userFile)))
                return null;
            if (solutionDir == null)
                return null;
            return GitService.GitServiceHelper.GetRepo(solutionDir);
        }
    }
}
