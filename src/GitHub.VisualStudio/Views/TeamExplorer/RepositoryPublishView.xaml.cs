using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels.TeamExplorer;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.TeamExplorer
{
    public class GenericRepositoryPublishView : ViewBase<IRepositoryPublishViewModel, RepositoryPublishView>
    { }
    
    [ExportViewFor(typeof(IRepositoryPublishViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryPublishView : GenericRepositoryPublishView
    {
        [ImportingConstructor]
        public RepositoryPublishView(ITeamExplorerServices teServices, INotificationDispatcher notifications)
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.PublishRepository, v => v.publishRepositoryButton));

                ViewModel.PublishRepository.Subscribe(state =>
                {
                    if (state == ProgressState.Success)
                    {
                        teServices.ShowMessage(GitHub.Resources.RepositoryPublishedMessage);
                    }
                });

                d(notifications.Listen()
                    .Where(n => n.Type == Notification.NotificationType.Error)
                    .Subscribe(n => teServices.ShowError(n.Message)));

                d(this.WhenAny(x => x.ViewModel.SafeRepositoryNameWarningValidator.ValidationResult, x => x.Value)
                    .WhereNotNull()
                    .Select(result => result?.Message)
                    .Subscribe(message =>
                    {
                        if (!String.IsNullOrEmpty(message))
                            teServices.ShowWarning(message);
                        else
                            teServices.ClearNotifications();
                    }));
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }
    }
}
