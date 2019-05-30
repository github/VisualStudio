/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.ViewModels.TeamExplorer;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.ComponentModelHost;
using GitHub.VisualStudio;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio.Threading;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.TeamExplorerSample.Sync
{
    /// <summary>
    /// Publish section.
    /// </summary>
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.GitCommits, Priority)]
    public class PublishSection : TeamExplorerBaseSection
    {
        public const string SectionId = "35B18474-005D-4A2A-9CCF-FFFFEB60F1F5";
        public const int Priority = 4;

        readonly Guid PushToRemoteSectionId = new Guid("99ADF41C-0022-4C03-B3C2-05047A3F6C2C");
        readonly Guid GitHubPublishSectionId = new Guid("92655B52-360D-4BF5-95C5-D9E9E596AC76");
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public PublishSection()
            : base()
        {
            this.Title = "Publish to GitHub";
            this.IsExpanded = true;
            this.IsBusy = false;
            this.SectionContent = new PublishView(){
                DataContext = this
            };
            this.View.ParentSection = this;
        }

        /// <summary>
        /// Get the view.
        /// </summary>
        protected PublishView View
        {
            get { return this.SectionContent as PublishView; }
        }

        public ICommand PublishToGitHub { get; set; }

        /// <summary>
        /// Initialize override.
        /// </summary>
        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            RefreshVisibility();

            if (ServiceProvider.GetService(typeof(ITeamExplorerPage)) is ITeamExplorerPage page)
            {
                if (page.GetSection(PushToRemoteSectionId) is ITeamExplorerSection pushToRemoteSection)
                {
                    pushToRemoteSection.PropertyChanged += Section_PropertyChanged;
                }

                if (page.GetSection(GitHubPublishSectionId) is ITeamExplorerSection gitHubPublishSection)
                {
                    gitHubPublishSection.PropertyChanged += Section_PropertyChanged;
                }
            }

            PublishToGitHub = new RelayCommand(o => DoPublishToGitHub().Forget());
        }

        async Task DoPublishToGitHub()
        {
            var componentModel = await Microsoft.VisualStudio.Shell.ServiceProvider.GetGlobalServiceAsync<SComponentModel, IComponentModel>();
            ShowPublishDialog(componentModel);
        }

        void ShowPublishDialog(IComponentModel componentModel)
        {
            /*
            var factory = ServiceProvider.GetService<IViewViewModelFactory>();
            var viewModel = ServiceProvider.GetService<IRepositoryPublishViewModel>();
            */

            var compositionServices = new CompositionServices();
            var compositionContainer = compositionServices.CreateVisualStudioCompositionContainer(componentModel.DefaultExportProvider);

            var factory = compositionContainer.GetExportedValue<IViewViewModelFactory>();
            var viewModel = compositionContainer.GetExportedValue<IRepositoryPublishViewModel>();

            var busy = viewModel.WhenAnyValue(x => x.IsBusy).Subscribe(x => IsBusy = x);

            var completed = viewModel.PublishRepository
                .Where(x => x == ProgressState.Success)
                .Subscribe(_ =>
                {
                    var teamExplorer =
                        VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(ITeamExplorer)) as ITeamExplorer;
                    teamExplorer?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);

                    // HandleCreatedRepo(ActiveRepo);
                });

            var view = factory.CreateView<IRepositoryPublishViewModel>();
            view.DataContext = viewModel;

            SectionContent = view;

            /*

            var view = factory.CreateView<IRepositoryPublishViewModel>();
            view.DataContext = viewModel;
            SectionContent = view;

            Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                    x => view.Unloaded += x,
                    x => view.Unloaded -= x)
                .Take(1)
                .Subscribe(_ =>
                {
                    busy.Dispose();
                    completed.Dispose();
                });

            */
        }

        void HandleCreatedRepo(LocalRepositoryModel newrepo)
        {
            var msg = String.Format(CultureInfo.CurrentCulture, Constants.Notification_RepoCreated, newrepo.Name, newrepo.CloneUrl);
            msg += " " + String.Format(CultureInfo.CurrentCulture, Constants.Notification_CreateNewProject, newrepo.LocalPath);
            ShowNotification(newrepo, msg);
        }

        private void ShowNotification(LocalRepositoryModel newrepo, string msg)
        {
//            var teServices = ServiceProvider.TryGetService<ITeamExplorerServices>();
//
//            teServices.ClearNotifications();
//            teServices.ShowMessage(
//                msg,
//                new RelayCommand(o =>
//                {
//                    var str = o.ToString();
//                    /* the prefix is the action to perform:
//                     * u: launch browser with url
//                     * c: launch create new project dialog
//                     * o: launch open existing project dialog 
//                    */
//                    var prefix = str.Substring(0, 2);
//                    if (prefix == "u:")
//                        OpenInBrowser(ServiceProvider.TryGetService<IVisualStudioBrowser>(), new Uri(str.Substring(2)));
//                    else if (prefix == "o:")
//                    {
//                        if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().OpenSolutionViaDlg(str.Substring(2), 1)))
//                            ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
//                    }
//                    else if (prefix == "c:")
//                    {
//                        var vsGitServices = ServiceProvider.TryGetService<IVSGitServices>();
//                        vsGitServices.SetDefaultProjectPath(newrepo.LocalPath);
//                        if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().CreateNewProjectViaDlg(null, null, 0)))
//                            ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
//                    }
//                })
//            );
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
                visible = IsSectionVisible(page, PushToRemoteSectionId) && !IsSectionVisible(page, GitHubPublishSectionId);
            }

            if (IsVisible != visible)
            {
                IsVisible = visible;
            }
        }
    }
}
