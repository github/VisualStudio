using GitHub.VisualStudio.Services;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio
{
    public abstract class PackageBase : Package
    {
        IServiceProvider serviceProvider;
        protected IServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set
            {
                serviceProvider = value;
            }
        }

        public PackageBase()
        {
            ServiceProvider = this;
        }

        public PackageBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected void AddTopLevelMenuItem(
            uint packageCommandId,
            EventHandler eventHandler)
        {
            var menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            var menuCommandId = new CommandID(GuidList.guidGitHubCmdSet, (int)packageCommandId);
            var menuItem = new MenuCommand(eventHandler, menuCommandId);
            menuCommandService.AddCommand(menuItem);
        }

        public T GetService<T>()
        {
            Debug.Assert(this.serviceProvider != null, "GetService<T> called before service provider is set");
            if (serviceProvider == null)
                return default(T);
            return (T)serviceProvider.GetService(typeof(T));
        }

        public Ret GetService<T, Ret>() where Ret : class
        {
            return GetService<T>() as Ret;
        }

        public T GetExportedValue<T>()
        {
            var componentModel = (IComponentModel)GetService<SComponentModel>();
            if (componentModel == null)
                return default(T);
            var exportProvider = componentModel.DefaultExportProvider;
            return exportProvider.GetExportedValue<T>();
        }

        protected void EnsureUIProvider()
        {
            var ui = GetExportedValue<UIProvider>();
            ui.EnsureProvider(GetService<SComponentModel, IComponentModel>().DefaultExportProvider);
        }

    }
}
