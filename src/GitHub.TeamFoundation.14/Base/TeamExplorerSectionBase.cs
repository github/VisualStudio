using System;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using GitHub.Services;
using System.Diagnostics;
using GitHub.Api;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerSectionBase : TeamExplorerItemBase, ITeamExplorerSection, IServiceProviderAware
    {
        protected IConnectionManager ConnectionManager { get; set; }

        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; this.RaisePropertyChange(); }
        }

        bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { isExpanded = value; this.RaisePropertyChange(); }
        }

        object sectionContent;
        public object SectionContent
        {
            get { return sectionContent; }
            set { sectionContent = value; this.RaisePropertyChange(); }
        }

        string title;
        public string Title
        {
            get { return title; }
            set { title = value; this.RaisePropertyChange(); }
        }

        public virtual object GetExtensibilityService(Type serviceType)
        {
            return null;
        }

        public TeamExplorerSectionBase(IGitHubServiceProvider serviceProvider, ITeamExplorerServiceHolder holder)
            : base(serviceProvider, holder)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(holder, nameof(holder));

            IsVisible = false;
            IsEnabled = true;
            IsExpanded = true;
        }

        public TeamExplorerSectionBase(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder)
            : base(serviceProvider, apiFactory, holder)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(apiFactory, nameof(apiFactory));
            Guard.ArgumentNotNull(holder, nameof(holder));

            IsVisible = false;
            IsEnabled = true;
            IsExpanded = true;
        }

        public TeamExplorerSectionBase(IGitHubServiceProvider serviceProvider,
            ITeamExplorerServiceHolder holder, IConnectionManager cm) : this(serviceProvider, holder)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(holder, nameof(holder));
            Guard.ArgumentNotNull(cm, nameof(cm));

            ConnectionManager = cm;
        }

        public TeamExplorerSectionBase(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm) : this(serviceProvider, apiFactory, holder)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(apiFactory, nameof(apiFactory));
            Guard.ArgumentNotNull(holder, nameof(holder));
            Guard.ArgumentNotNull(cm, nameof(cm));

            ConnectionManager = cm;
        }

        void ITeamExplorerSection.Cancel()
        {
        }

        void ITeamExplorerSection.Initialize(object sender, SectionInitializeEventArgs e)
        {
            Guard.ArgumentNotNull(e, nameof(e));

            Initialize(e.ServiceProvider);
        }

        public virtual void Loaded(object sender, SectionLoadedEventArgs e)
        {
        }

        public virtual void Refresh()
        {
        }

        public virtual void SaveContext(object sender, SectionSaveContextEventArgs e)
        {
        }

        protected ITeamExplorerSection GetSection(Guid section)
        {
            // Return null if section hasn't been initialized yet
            var tep = (ITeamExplorerPage)TEServiceProvider?.GetServiceSafe(typeof(ITeamExplorerPage));
            return tep?.GetSection(section);
        }
    }
}
