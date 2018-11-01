using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GitHub.Factories;
using GitHub.Services;
using GitHub.ViewModels;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using ReactiveUI;

namespace GitHub.VisualStudio.UI
{
    public class AsyncPaneBase<TViewModel> : ToolWindowPane
        where TViewModel : IPaneViewModel
    {
        readonly ContentPresenter contentPresenter;
        IDisposable subscription;
        JoinableTask<TViewModel> viewModelTask;

        public AsyncPaneBase()
        {
            Content = contentPresenter = new ContentPresenter();
        }

        public virtual FrameworkElement View
        {
            get => (FrameworkElement)contentPresenter.Content;
            set => contentPresenter.Content = value;
        }

        protected override void Initialize()
        {
            // Using JoinableTaskFactory from parent AsyncPackage. That way if VS shuts down before this
            // work is done, we won't risk crashing due to arbitrary work going on in background threads.
            var asyncPackage = (AsyncPackage)Package;
            viewModelTask = asyncPackage.JoinableTaskFactory.RunAsync(() => InitializeAsync(asyncPackage));
        }

        public Task<TViewModel> GetViewModelAsync() => viewModelTask.JoinAsync();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                subscription?.Dispose();
                subscription = null;
            }
        }

        async Task<TViewModel> InitializeAsync(AsyncPackage asyncPackage)
        {
            try
            {
                // Allow MEF to initialize its cache asynchronously
                var provider = (IGitHubServiceProvider)await asyncPackage.GetServiceAsync(typeof(IGitHubServiceProvider));
                var teServiceHolder = provider.GetService<ITeamExplorerServiceHolder>();
                teServiceHolder.ServiceProvider = this;
                var factory = provider.GetService<IViewViewModelFactory>();
                var viewModel = provider.ExportProvider.GetExportedValue<TViewModel>();
                await viewModel.InitializeAsync(this);
                View = factory.CreateView<TViewModel>();
                if (View == null)
                {
                    throw new CompositionException("Could not find view for " + typeof(TViewModel).FullName);
                }
                View.DataContext = viewModel;
                subscription = viewModel.WhenAnyValue(x => x.PaneCaption).Subscribe(x => Caption = x);
                return viewModel;
            }
            catch (Exception e)
            {
                ShowError(e);
                throw;
            }
        }

        void ShowError(Exception e)
        {
            View = new TextBox
            {
                Text = e.ToString(),
                IsReadOnly = true,
            };
        }
    }
}