using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Controls;
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
using GitHub.Authentication;
using System.Reactive.Subjects;
using GitHub.Primitives;
using System.Collections.Specialized;
using System.ComponentModel;

public class UIControllerTests
{
    public class TheDisposeMethod : TestBaseClass
    {
        [Fact]
        public void WithMultipleCallsDoesNotThrowException()
        {
            var uiProvider = Substitute.For<IUIProvider>();
            var hosts = Substitute.For<IRepositoryHosts>();
            var factory = Substitute.For<IExportFactoryProvider>();
            var cm = Substitutes.ConnectionManager;
            var uiController = new UIController(uiProvider, hosts, factory, cm);

            uiController.Dispose();
            uiController.Dispose();
        }
    }

    public class TheStartMethod : TestBaseClass
    {
        void SetupView<VM>(IExportFactoryProvider factory, GitHub.Exports.UIViewType type)
            where VM : class, IViewModel
        {
            IView view;
            if (type == GitHub.Exports.UIViewType.PRList)
                view = Substitutes.For<IView, IViewFor<VM>, IHasCreationView, IHasDetailView>();
            else
                view = Substitute.For<IView, IViewFor<VM>>();

            view.Done.Returns(new ReplaySubject<object>());
            view.Cancel.Returns(new ReplaySubject<object>());

            (view as IHasDetailView)?.Open.Returns(new ReplaySubject<object>());
            (view as IHasCreationView)?.Create.Returns(new ReplaySubject<object>());

            var e = new ExportLifetimeContext<IView>(view, () => {}) ;
            factory.GetView(type).Returns(e);
        }

        void SetupViewModel<VM>(IExportFactoryProvider factory, GitHub.Exports.UIViewType type)
            where VM : class, IViewModel
        {
            var v = Substitute.For<VM, INotifyPropertyChanged>();
            var e = new ExportLifetimeContext<IViewModel>(v, () => { });

            factory.GetViewModel(type).Returns(e);
        }

        void RaisePropertyChange(object obj, string prop)
        {
            (obj as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(prop));
        }

        IExportFactoryProvider SetupFactory(IServiceProvider provider)
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

            SetupView<ILoginControlViewModel>(factory, GitHub.Exports.UIViewType.Login);
            SetupView<ITwoFactorDialogViewModel>(factory, GitHub.Exports.UIViewType.TwoFactor);
            SetupView<IRepositoryCloneViewModel>(factory, GitHub.Exports.UIViewType.Clone);
            SetupView<IRepositoryCreationViewModel>(factory, GitHub.Exports.UIViewType.Create);
            SetupView<IRepositoryPublishViewModel>(factory, GitHub.Exports.UIViewType.Publish);
            SetupView<IPullRequestListViewModel>(factory, GitHub.Exports.UIViewType.PRList);
            SetupView<IPullRequestDetailViewModel>(factory, GitHub.Exports.UIViewType.PRDetail);
            SetupView<IPullRequestCreationViewModel>(factory, GitHub.Exports.UIViewType.PRCreation);

            return factory;
        }

        [Fact]
        public void ShowingCloneDialogWithoutBeingLoggedInShowsLoginDialog()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var loginView = factory.GetView(GitHub.Exports.UIViewType.Login);
            loginView.Value.Cancel.Returns(Observable.Empty<object>());
            var cm = provider.GetConnectionManager();
            var cons = new System.Collections.ObjectModel.ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                var list = new List<IView>();
                uiController.SelectFlow(UIControllerFlow.Clone)
                    .Subscribe(uc => list.Add(uc as IView),
                                () =>
                                {
                                    Assert.Equal(1, list.Count);
                                    Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(list[0]);
                                });

                uiController.Start(null);
            }
        }

        [Fact]
        public void ShowingCloneDialogWhenLoggedInShowsCloneDialog()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var connection = provider.GetConnection();
            connection.Login().Returns(Observable.Return(connection));
            var cm = provider.GetConnectionManager();
            var host = hosts.GitHubHost;
            hosts.LookupHost(connection.HostAddress).Returns(host);
            host.IsLoggedIn.Returns(true);

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                var list = new List<IView>();
                uiController.SelectFlow(UIControllerFlow.Clone)
                    .Subscribe(uc => list.Add(uc as IView),
                                () =>
                                {
                                    Assert.Equal(1, list.Count);
                                    Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(list[0]);
                                });
                uiController.Start(connection);
            }
        }

        [Fact]
        public void PullRequestFlowWhenLoggedOut()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new System.Collections.ObjectModel.ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var list = new List<IView>();
                var flow = uiController.SelectFlow(UIControllerFlow.PullRequests);
                flow.Subscribe(uc =>
                    {
                        switch (++count)
                        {
                            case 1:
                                Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                                var vm = factory.GetViewModel(GitHub.Exports.UIViewType.TwoFactor);
                                vm.Value.IsShowing.Returns(true);
                                RaisePropertyChange(vm.Value, "IsShowing");
                                break;
                            case 2:
                                Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);
                                var con = Substitutes.Connection;
                                var host = Substitute.For<IRepositoryHost>();
                                host.IsLoggedIn.Returns(true);
                                con.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
                                hosts.LookupHost(Args.HostAddress).Returns(host);
                                cons.Add(con);
                                var v = factory.GetView(GitHub.Exports.UIViewType.Login).Value;
                                ((ReplaySubject<object>)v.Done).OnNext(null);
                                break;
                            case 3:
                                Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);
                                ((ReplaySubject<object>)(uc as IHasDetailView).Open).OnNext(null);
                                break;
                            case 4:
                                Assert.IsAssignableFrom<IViewFor<IPullRequestDetailViewModel>>(uc);
                                ((ReplaySubject<object>)(uc as IView).Cancel).OnNext(null);
                                break;
                            case 5:
                                Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);
                                ((ReplaySubject<object>)(uc as IHasCreationView).Create).OnNext(null);
                                break;
                            case 6:
                                Assert.IsAssignableFrom<IViewFor<IPullRequestCreationViewModel>>(uc);
                                uiController.Stop();
                                break;
                        }
                    });

                uiController.Start(null);
                Assert.Equal(6, count);
                Assert.True(uiController.IsStopped);
            }
        }


        [Fact]
        public void GoingBackAndForthOnTheLoginFlow()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new System.Collections.ObjectModel.ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                var count = 0;
                var list = new List<IView>();
                var flow = uiController.SelectFlow(UIControllerFlow.PullRequests);
                flow.Subscribe(uc =>
                {
                    switch (++count)
                    {
                        case 1:
                        {
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            var vm = factory.GetViewModel(GitHub.Exports.UIViewType.TwoFactor);
                            vm.Value.IsShowing.Returns(true);
                            RaisePropertyChange(vm.Value, "IsShowing");
                        }
                        break;
                        case 2:
                        {
                            Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);
                            var vm = factory.GetViewModel(GitHub.Exports.UIViewType.TwoFactor);
                            vm.Value.IsShowing.Returns(false);
                            RaisePropertyChange(vm.Value, "IsShowing");
                            ((ReplaySubject<object>)(uc as IView).Cancel).OnNext(null);
                        }
                        break;
                        case 3:
                        {
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            var vm = factory.GetViewModel(GitHub.Exports.UIViewType.TwoFactor);
                            vm.Value.IsShowing.Returns(true);
                            RaisePropertyChange(vm.Value, "IsShowing");
                        }
                        break;
                        case 4:
                        {
                            Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);
                            uiController.Stop();
                        }
                        break;
                    }
                });

                uiController.Start(null);
                Assert.Equal(4, count);
                Assert.True(uiController.IsStopped);
            }
        }
    }
}
