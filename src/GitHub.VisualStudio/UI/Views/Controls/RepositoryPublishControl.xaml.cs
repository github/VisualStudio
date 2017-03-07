using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using System.ComponentModel.Composition;
using GitHub.Extensions.Reactive;
using GitHub.Services;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericRepositoryPublishControl : SimpleViewUserControl<IRepositoryPublishViewModel, RepositoryPublishControl>
    { }
    
    [ExportView(ViewType=UIViewType.Publish)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryPublishControl : GenericRepositoryPublishControl, IDialogView
    {
        [ImportingConstructor]
        public RepositoryPublishControl(ITeamExplorerServices teServices, INotificationDispatcher notifications)
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.PublishRepository, v => v.publishRepositoryButton));

                ViewModel.PublishRepository.Subscribe(state =>
                {
                    if (state == ProgressState.Success)
                    {
                        teServices.ShowMessage(UI.Resources.RepositoryPublishedMessage);
                        NotifyDone();
                    }
                });

                d(this.WhenAny(x => x.ViewModel.IsPublishing, x => x.Value)
                    .Subscribe(x => NotifyIsBusy(x)));


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
