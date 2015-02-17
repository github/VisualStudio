using GitHub.VisualStudio.Base;
using Microsoft.TeamFoundation.Controls;
using NullGuard;
using System;
using Microsoft.TeamFoundation.Client;

namespace GitHub.VisualStudio
{
    public class TeamExplorerSectionBase : TeamExplorerGitAwareItem, ITeamExplorerSection, INotifyPropertySource
    {
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

        // When this class goes back to inheriting from TeamExplorerBase,
        // this property should be restored
        /*
        bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; this.RaisePropertyChange(); }
        }
        */

        object sectionContent;
        [AllowNull]
        public object SectionContent
        {
            get { return sectionContent; }
            set { sectionContent = value; this.RaisePropertyChange(); }
        }

        string title;
        [AllowNull]
        public string Title
        {
            get { return title; }
            set { title = value; this.RaisePropertyChange(); }
        }

        public virtual void Cancel()
        {
        }

        [return: AllowNull]
        public virtual object GetExtensibilityService(Type serviceType)
        {
            return null;
        }

        public virtual void Initialize(object sender, SectionInitializeEventArgs e)
        {
            ServiceProvider = e.ServiceProvider;
            Initialize();
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

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            if (e.TeamProjectChanged)
            {
                if (e.NewContext != null && e.NewContext.HasTeamProject)
                    IsVisible = true;
                else
                    IsVisible = false;
            }
            base.ContextChanged(sender, e);
        }
    }
}
