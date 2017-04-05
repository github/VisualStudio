using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Controllers;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NSubstitute;
using Xunit;
using UnitTests;
using GitHub.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Subjects;
using GitHub.Primitives;
using System.ComponentModel;
using System.Collections.ObjectModel;
using GitHub.App.Factories;
using System.Reactive;
using GitHub.Extensions.Reactive;

public class UIControllerTests
{
    public class TheDisposeMethod : TestBaseClass
    {
        [Fact]
        public void WithMultipleCallsDoesNotThrowException()
        {
            var uiProvider = Substitute.For<IGitHubServiceProvider>();
            var hosts = Substitute.For<IRepositoryHosts>();
            var factory = Substitute.For<IUIFactory>();
            var cm = Substitutes.ConnectionManager;
            var uiController = new UIController(uiProvider, hosts, factory, cm);

            uiController.Dispose();
            uiController.Dispose();
        }
    }

    public class UIControllerTestBase : TestBaseClass
    {
        protected void SetupView<VM>(IExportFactoryProvider factory, GitHub.Exports.UIViewType type)
            where VM : class, IViewModel
        {
            var view = Substitute.For<IView, IViewFor<VM>>();
            var viewModel = factory.GetViewModel(type).Value;
            view.ViewModel.Returns(viewModel);

            var e = new ExportLifetimeContext<IView>(view, () => { });
            factory.GetView(type).Returns(e);
        }

        protected void SetupViewModel<VM>(IExportFactoryProvider factory, GitHub.Exports.UIViewType type)
            where VM : class, IViewModel
        {
            var v = Substitute.For<IDialogViewModel, VM, INotifyPropertyChanged>();
            var done = new ReplaySubject<Unit>();
            var cancel = ReactiveCommand.Create();
            v.Done.Returns(_ => done);
            v.Cancel.Returns(cancel);
            ((IHasCancel)v).Cancel.Returns(cancel.SelectUnit());

            var e = new ExportLifetimeContext<IViewModel>(v, () => { });

            factory.GetViewModel(type).Returns(e);
        }

        protected void RaisePropertyChange(object obj, string prop)
        {
            (obj as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(prop));
        }

        protected IUIFactory SetupFactory(IServiceProvider provider)
        {
            var factory = provider.GetExportFactoryProvider();
            SetupViewModel<ILoginControlViewModel>(factory, GitHub.Exports.UIViewType.Login);
            SetupViewModel<ITwoFactorDialogViewModel>(factory, GitHub.Exports.UIViewType.TwoFactor);
            SetupViewModel<IRepositoryCloneViewModel>(factory, GitHub.Exports.UIViewType.Clone);
            SetupViewModel<IRepositoryCreationViewModel>(factory, GitHub.Exports.UIViewType.Create);
            SetupViewModel<IRepositoryPublishViewModel>(factory, GitHub.Exports.UIViewType.Publish);
            SetupViewModel<IPullRequestListViewModel>(factory, GitHub.Exports.UIViewType.PRList);
            SetupViewModel<IPullRequestDetailViewModel>(factory, GitHub.Exports.UIViewType.PRDetail);
            SetupViewModel<IPullRequestCreationViewModel>(factory, GitHub.Exports.UIViewType.PRCreation);
            SetupViewModel<IGistCreationViewModel>(factory, GitHub.Exports.UIViewType.Gist);
            SetupViewModel<ILogoutRequiredViewModel>(factory, GitHub.Exports.UIViewType.LogoutRequired);


            SetupView<ILoginControlViewModel>(factory, GitHub.Exports.UIViewType.Login);
            SetupView<ITwoFactorDialogViewModel>(factory, GitHub.Exports.UIViewType.TwoFactor);
            SetupView<IRepositoryCloneViewModel>(factory, GitHub.Exports.UIViewType.Clone);
            SetupView<IRepositoryCreationViewModel>(factory, GitHub.Exports.UIViewType.Create);
            SetupView<IRepositoryPublishViewModel>(factory, GitHub.Exports.UIViewType.Publish);
            SetupView<IPullRequestListViewModel>(factory, GitHub.Exports.UIViewType.PRList);
            SetupView<IPullRequestDetailViewModel>(factory, GitHub.Exports.UIViewType.PRDetail);
            SetupView<IPullRequestCreationViewModel>(factory, GitHub.Exports.UIViewType.PRCreation);
            SetupView<IGistCreationViewModel>(factory, GitHub.Exports.UIViewType.Gist);
            SetupView<ILogoutRequiredViewModel>(factory, GitHub.Exports.UIViewType.LogoutRequired);

            return new UIFactory(factory);
        }

        protected IConnection SetupConnection(IServiceProvider provider, IRepositoryHosts hosts,
            IRepositoryHost host, bool loggedIn = true, bool supportsGist = true)
        {
            var connection = provider.GetConnection();
            connection.Login().Returns(Observable.Return(connection));
            hosts.LookupHost(connection.HostAddress).Returns(host);
            host.IsLoggedIn.Returns(loggedIn);
            host.SupportsGist.Returns(supportsGist);
            return connection;
        }

        protected void TriggerCancel(IViewModel viewModel)
        {
            ((IDialogViewModel)viewModel).Cancel.Execute(null);
        }

        protected void TriggerDone(IViewModel viewModel)
        {
            var hasDone = (IHasDone)viewModel;
            ((ISubject<Unit>)hasDone.Done).OnNext(Unit.Default);
        }
    }

    public class AuthFlow : UIControllerTestBase
    {
        [Fact]
        public void RunningNonAuthFlowWithoutBeingLoggedInRunsAuthFlow()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Clone);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            TriggerCancel(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(1, count);
                Assert.True(uiController.IsStopped);
            }
        }

        [Fact]
        public void RunningNonAuthFlowWhenLoggedInRunsNonAuthFlow()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            // simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Clone);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);
                            TriggerCancel(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(1, count);
                Assert.True(uiController.IsStopped);
            }
        }

        [Fact]
        public void RunningAuthFlowWithoutBeingLoggedInRunsAuthFlow()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Authentication);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            TriggerCancel(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(1, count);
                Assert.True(uiController.IsStopped);
            }
        }

        [Fact]
        public void RunningAuthFlowWhenLoggedInRunsAuthFlow()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();

            // simulate being logged in
            var host = hosts.GitHubHost;
            var connection = SetupConnection(provider, hosts, host);
            var cons = new ObservableCollection<IConnection> { connection };
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Authentication);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            TriggerCancel(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(1, count);
                Assert.True(uiController.IsStopped);
            }
        }

        [Fact]
        public void AuthFlowWithout2FA()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Clone);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            // login
                            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));
                            TriggerDone(data.View.ViewModel);
                            break;
                        case 2:
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);
                            TriggerCancel(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(2, count);
                Assert.True(uiController.IsStopped);
            }
        }

        [Fact]
        public void AuthFlowWith2FA()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Clone);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            var vm = (IDialogViewModel)factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.TwoFactor).ViewModel;
                            vm.IsShowing.Returns(true);
                            RaisePropertyChange(vm, "IsShowing");
                            break;
                        case 2:
                            Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);
                            // login
                            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));
                            // continue by triggering done on login view
                            var vm2 = factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.Login).ViewModel;
                            TriggerDone(vm2);
                            break;
                        case 3:
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);
                            TriggerCancel(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(3, count);
                Assert.True(uiController.IsStopped);
            }
        }

        [Fact]
        public void BackAndForth()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Clone);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1: {
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            var vm = (IDialogViewModel)factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.TwoFactor).ViewModel;
                            vm.IsShowing.Returns(true);
                            RaisePropertyChange(vm, "IsShowing");
                            break;
                        }
                        case 2: {
                            Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);
                            var vm = (IDialogViewModel)factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.TwoFactor).ViewModel;
                            vm.IsShowing.Returns(false);
                            RaisePropertyChange(vm, "IsShowing");
                            TriggerCancel(data.View.ViewModel);
                            break;
                        }
                        case 3: {
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            var vm = (IDialogViewModel)factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.TwoFactor).ViewModel;
                            vm.IsShowing.Returns(true);
                            RaisePropertyChange(vm, "IsShowing");
                            break;
                        }
                        case 4: {
                            Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);
                            // login
                            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));
                            var vm2 = factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.Login).ViewModel;
                            TriggerDone(vm2);
                            break;
                        }
                        case 5: {
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);
                            uiController.Stop();
                            break;
                        }
                    }
                });

                uiController.Start();
                Assert.Equal(5, count);
                Assert.True(uiController.IsStopped);
            }
        }
    }

    public class CloneFlow : UIControllerTestBase
    {
        [Fact]
        public void Flow()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            // simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Clone);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);
                            TriggerDone(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(1, count);
                Assert.True(uiController.IsStopped);
            }
        }
    }

    public class CreateFlow : UIControllerTestBase
    {
        [Fact]
        public void Flow()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            // simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Create);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCreationViewModel>>(uc);
                            TriggerDone(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(1, count);
                Assert.True(uiController.IsStopped);
            }
        }
    }

    public class PublishFlow : UIControllerTestBase
    {
        [Fact]
        public void FlowWithConnection()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);
            var connection = SetupConnection(provider, hosts, hosts.GitHubHost);

            // simulate being logged in
            cons.Add(connection);

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Publish, connection);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<IRepositoryPublishViewModel>>(uc);
                            ((IGitHubServiceProvider)provider).Received().AddService(uiController, connection);
                            TriggerDone(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(1, count);
                Assert.True(uiController.IsStopped);
            }
        }

        [Fact]
        public void FlowWithoutConnection()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);
            var connection = SetupConnection(provider, hosts, hosts.GitHubHost);

            // simulate being logged in
            cons.Add(connection);

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var flow = uiController.Configure(UIControllerFlow.Publish);
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<IRepositoryPublishViewModel>>(uc);
                            ((IGitHubServiceProvider)provider).Received().AddService(uiController, connection);
                            TriggerDone(data.View.ViewModel);
                            break;
                    }
                });

                uiController.Start();
                Assert.Equal(1, count);
                Assert.True(uiController.IsStopped);
            }
        }
    }

    public class GistFlow : UIControllerTestBase
    {
        [Fact]
        public void ShowingGistDialogWhenGistNotSupportedShowsLogoutDialog()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            var host = hosts.GitHubHost;
            // simulate being logged in
            cons.Add(SetupConnection(provider, hosts, host, true, false));

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                bool? success = null;
                var flow = uiController.Configure(UIControllerFlow.Gist);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(3, count);
                        count++;
                    });

                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<ILogoutRequiredViewModel>>(uc);
                            host.IsLoggedIn.Returns(false);
                            TriggerDone(data.View.ViewModel);
                            break;
                        case 2:
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            // login
                            host.IsLoggedIn.Returns(true);
                            host.SupportsGist.Returns(true);
                            TriggerDone(data.View.ViewModel);
                            break;
                        case 3:
                            Assert.IsAssignableFrom<IViewFor<IGistCreationViewModel>>(uc);
                            TriggerDone(data.View.ViewModel);
                            break;
                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(4, count);
                    count++;
                });

                uiController.Start();
                Assert.Equal(5, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
            }
        }

        [Fact]
        public void ShowingGistDialogWhenGistSupportedShowsGistDialog()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            // simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost, true, true));

            using (var uiController = new UIController((IGitHubServiceProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                bool? success = null;
                var flow = uiController.Configure(UIControllerFlow.Gist);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(1, count);
                        count++;
                    });

                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            Assert.IsAssignableFrom<IViewFor<IGistCreationViewModel>>(uc);
                            TriggerDone(data.View.ViewModel);
                            break;
                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(2, count);
                    count++;
                });

                uiController.Start();
                Assert.Equal(3, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
            }
        }
    }
}