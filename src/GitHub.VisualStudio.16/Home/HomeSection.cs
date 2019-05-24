/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/
using System;
using System.ComponentModel;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;

namespace Microsoft.TeamExplorerSample.Sync
{
    /// <summary>
    /// Publish section.
    /// </summary>
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.Home, Priority)]
    public class HomeSection : TeamExplorerBaseSection
    {
        public const string SectionId = "C655016C-CDCF-4E04-8B8F-DD769B740A8A";
        public const int Priority = 10;

        readonly static Guid GitHubHomeSectionId = new Guid("72008232-2104-4FA0-A189-61B0C6F91198");

        /// <summary>
        /// Constructor.
        /// </summary>
        public HomeSection()
            : base()
        {
            Title = "GitHub";
            this.IsExpanded = true;
            this.IsBusy = false;
            View = new GitHubHomeContent();
            View.DataContext = this;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            RefreshVisibility();

            if (ServiceProvider.GetService(typeof(ITeamExplorerPage)) is ITeamExplorerPage page)
            {
                if (page.GetSection(GitHubHomeSectionId) is ITeamExplorerSection githubHomeSection)
                {
                    githubHomeSection.PropertyChanged += Section_PropertyChanged;
                }
            }
        }

        void Section_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ITeamExplorerSection.IsVisible):
                case nameof(ITeamExplorerSection.SectionContent):
                    RefreshVisibility();
                    break;
            }
        }

        void RefreshVisibility()
        {
            bool IsSectionVisible(ITeamExplorerPage teamExplorerPage, Guid sectionId)
            {
                if (teamExplorerPage.GetSection(sectionId) is ITeamExplorerSection pushToRemoteSection)
                {
                    return pushToRemoteSection.SectionContent != null && pushToRemoteSection.IsVisible;
                }

                return false;
            }

            var visible = false;

            if (ServiceProvider.GetService(typeof(ITeamExplorerPage)) is ITeamExplorerPage page)
            {
                visible = !IsSectionVisible(page, GitHubHomeSectionId);
            }

            if (IsVisible != visible)
            {
                IsVisible = visible;
            }
        }

        public override async void Refresh()
        {
            base.Refresh();
        }

        protected GitHubHomeContent View
        {
            get { return SectionContent as GitHubHomeContent; }
            set { SectionContent = value; }
        }
    }
}
