/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.VisualStudio.Annotations;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using ReactiveUI;

namespace Microsoft.TeamExplorerSample.Sync
{
    /// <summary>
    /// Publish section.
    /// </summary>
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.Home, Priority)]
    public class HomeSection : TeamExplorerBaseSection
    {
        readonly CompositionServices compositionServices;

        public const string SectionId = "C655016C-CDCF-4E04-8B8F-DD769B740A8A";
        public const int Priority = 10;

        /// <summary>
        /// Constructor.
        /// </summary>
        [ImportingConstructor]
        public HomeSection(CompositionServices compositionServices)
            : base()
        {
            this.compositionServices = compositionServices;

            Title = "GitHub";
            this.IsExpanded = true;
            this.IsBusy = false;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            var isInstalled = FullExtensionUtilities.IsInstalled(ServiceProvider);

            if (isInstalled)
            {
                IsVisible = false;
                return;
            }

            var exportProvider = this.compositionServices.GetExportProvider();
            var homeSectionViewModel = exportProvider.GetExportedValue<HomeSectionViewModel>();

            View = new HomeSectionView
            {
                DataContext = homeSectionViewModel
            };
        }

        public override async void Refresh()
        {
            base.Refresh();
        }

        protected HomeSectionView View
        {
            get { return SectionContent as HomeSectionView; }
            set { SectionContent = value; }
        }
    }

    [Export]
    public class HomeSectionViewModel: INotifyPropertyChanged
    {
        UriString cloneUrl;

        [ImportingConstructor]
        public HomeSectionViewModel(CompositionServices compositionServices)
        {
            var exportProvider = compositionServices.GetExportProvider();
            var teamExplorerContext = exportProvider.GetExportedValue<ITeamExplorerContext>();
            teamExplorerContext.PropertyChanged += TeamExplorerContextOnPropertyChanged;
        }

        void TeamExplorerContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var teamExplorerContext = (ITeamExplorerContext) sender;
            if (e.PropertyName == nameof(ITeamExplorerContext.ActiveRepository))
            {
                if (teamExplorerContext.ActiveRepository != null)
                {
                    this.CloneUrl = teamExplorerContext.ActiveRepository.CloneUrl;
                }
            }
        }

        public UriString CloneUrl
        {
            get { return cloneUrl; }
            set
            {
                if (cloneUrl != value)
                {
                    cloneUrl = value;
                    OnPropertyChanged(nameof(CloneUrl));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
