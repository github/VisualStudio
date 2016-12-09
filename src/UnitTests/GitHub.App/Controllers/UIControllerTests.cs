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

public class UIControllerTests
{
    public class TheDisposeMethod : TestBaseClass
    {
        [Fact]
        public void WithMultipleCallsDoesNotThrowException()
        {
            var uiProvider = Substitute.For<IUIProvider>();
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
            IView view;
            if (type == GitHub.Exports.UIViewType.PRList)
                view = Substitutes.For<IView, IViewFor<VM>, IHasCreationView, IHasDetailView>();
            else
                view = Substitute.For<IView, IViewFor<VM>>();

            view.Done.Returns(new ReplaySubject<ViewWithData>());
            view.Cancel.Returns(new ReplaySubject<ViewWithData>());

            (view as IHasDetailView)?.Open.Returns(new ReplaySubject<ViewWithData>());
            (view as IHasCreationView)?.Create.Returns(new ReplaySubject<ViewWithData>());

            var e = new ExportLifetimeContext<IView>(view, () => { });
            factory.GetView(type).Returns(e);
        }

        protected void SetupViewModel<VM>(IExportFactoryProvider factory, GitHub.Exports.UIViewType type)
            where VM : class, IViewModel
        {
            var v = Substitute.For<VM, INotifyPropertyChanged>();
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

        protected void TriggerDetailViewOpen(IView uc, ViewWithData viewWithData)
        {
            ((ReplaySubject<ViewWithData>)((IHasDetailView)uc).Open).OnNext(viewWithData);
        }

        protected void TriggerCreationViewCreate(IView uc, ViewWithData viewWithData)
        {
            ((ReplaySubject<ViewWithData>)((IHasCreationView)uc).Create).OnNext(viewWithData);
        }

        protected void TriggerCancel(IView view)
        {
            ((ReplaySubject<ViewWithData>)view.Cancel).OnNext(null);
        }

        protected void TriggerDone(IView view)
        {
            ((ReplaySubject<ViewWithData>)view.Done).OnNext(null);
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

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Clone flow
                var flow = uiController.SelectFlow(UIControllerFlow.Clone);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is ILoginControlViewModel
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);

                            //Cancelling Clone flow
                            TriggerCancel(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.False(success);
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

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Clone flow
                var flow = uiController.SelectFlow(UIControllerFlow.Clone);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IRepositoryCloneViewModel
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);

                            //Cancelling Clone flow
                            TriggerCancel(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.False(success);
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

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Authentication flow
                var flow = uiController.SelectFlow(UIControllerFlow.Authentication);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is ILoginControlViewModel
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);

                            //Cancelling Authentication flow
                            TriggerCancel(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.False(success);
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

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Authentication flow
                var flow = uiController.SelectFlow(UIControllerFlow.Authentication);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is ILoginControlViewModel
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);

                            //Cancelling Authentication flow
                            TriggerCancel(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.False(success);
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

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 2;

                var count = 0;
                bool? success = null;

                //Starting Clone flow
                var flow = uiController.SelectFlow(UIControllerFlow.Clone);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is ILoginControlViewModel
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                          
                            //Login
                            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

                            TriggerDone(uc);
                            break;
                        case 2:
                            //Demonstrate view is IRepositoryCloneViewModel
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);

                            //Cancelling Clone flow
                            TriggerCancel(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
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

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 3;

                var count = 0;
                bool? success = null;

                //Starting Clone flow
                var flow = uiController.SelectFlow(UIControllerFlow.Clone);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is ILoginControlViewModel
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);

                            //Displaying the TwoFactorView
                            var vm = factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.TwoFactor).ViewModel;
                            vm.IsShowing.Returns(true);
                            RaisePropertyChange(vm, "IsShowing");

                            break;
                        case 2:
                            //Demonstrate view is ITwoFactorDialogViewModel
                            Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);

                            //Login
                            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));
                            
                            //Continue by triggering done on login view
                            var v = factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.Login).View;
                            TriggerDone(v);

                            break;
                        case 3:
                            //Demonstrate view is IRepositoryCloneViewModel
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);

                            //Cancelling Clone flow
                            TriggerCancel(uc);

                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
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

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 5;

                var count = 0;
                bool? success = null;

                //Starting Clone flow
                var flow = uiController.SelectFlow(UIControllerFlow.Clone);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            {
                                //Demonstrate view is ILoginControlViewModel
                                Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);

                                //Displaying the TwoFactorView
                                var vm = factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.TwoFactor).ViewModel;
                                vm.IsShowing.Returns(true);
                                RaisePropertyChange(vm, "IsShowing");

                                break;
                            }
                        case 2:
                            {
                                //Demonstrate view is ITwoFactorDialogViewModel
                                Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);

                                //Hiding the TwoFactorView
                                var vm = factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.TwoFactor).ViewModel;
                                vm.IsShowing.Returns(false);
                                RaisePropertyChange(vm, "IsShowing");

                                //Cancelling the TwoFactorDialog
                                TriggerCancel(uc);

                                break;
                            }
                        case 3:
                            {
                                //Demonstrate view is ILoginControlViewModel
                                Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);

                                //Displaying the TwoFactorView
                                var vm = factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.TwoFactor).ViewModel;
                                vm.IsShowing.Returns(true);
                                RaisePropertyChange(vm, "IsShowing");

                                break;
                            }
                        case 4:
                            {
                                //Demonstrate view is ITwoFactorDialogViewModel
                                Assert.IsAssignableFrom<IViewFor<ITwoFactorDialogViewModel>>(uc);
                             
                                //Login
                                cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

                                //Completing the TwoFactorDialog
                                var v = factory.CreateViewAndViewModel(GitHub.Exports.UIViewType.Login).View;
                                TriggerDone(v);

                                break;
                            }
                        case 5:
                            {
                                //Demonstrate view is IRepositoryCloneViewModel
                                Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);

                                //Stopping UIController
                                uiController.Stop();

                                break;
                            }
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
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

            //Simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Clone flow
                var flow = uiController.SelectFlow(UIControllerFlow.Clone);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IRepositoryCloneViewModel
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(uc);

                            //Completing Create flow
                            TriggerDone(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
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

            //Simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Create flow
                var flow = uiController.SelectFlow(UIControllerFlow.Create);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IRepositoryCreationViewModel
                            Assert.IsAssignableFrom<IViewFor<IRepositoryCreationViewModel>>(uc);

                            //Completing Create flow
                            TriggerDone(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
            }
        }
    }

    public class PublishFlow : UIControllerTestBase
    {
        [Fact]
        public void FlowWithConnection()
        {
            var provider = (IUIProvider)Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);
            var connection = SetupConnection(provider, hosts, hosts.GitHubHost);

            //Simulate being logged in
            cons.Add(connection);

            using (var uiController = new UIController(provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Publish flow
                var flow = uiController.SelectFlow(UIControllerFlow.Publish);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IRepositoryPublishViewModel
                            Assert.IsAssignableFrom<IViewFor<IRepositoryPublishViewModel>>(uc);

                            provider.Received().AddService(uiController, connection);

                            //Completing Publish flow
                            TriggerDone(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(connection);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
            }
        }

        [Fact]
        public void FlowWithoutConnection()
        {
            var provider = (IUIProvider)Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);
            var connection = SetupConnection(provider, hosts, hosts.GitHubHost);

            //Simulate being logged in
            cons.Add(connection);

            using (var uiController = new UIController(provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Publish flow
                var flow = uiController.SelectFlow(UIControllerFlow.Publish);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IRepositoryPublishViewModel
                            Assert.IsAssignableFrom<IViewFor<IRepositoryPublishViewModel>>(uc);

                            provider.Received().AddService(uiController, connection);
                         
                            //Completing Publish flow
                            TriggerDone(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
            }
        }
    }

    public class PullRequestsFlow : UIControllerTestBase
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

            //Simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 9;

                var testPullRequestViewWithData = new ViewWithData(UIControllerFlow.PullRequests)
                {
                    ViewType = GitHub.Exports.UIViewType.PRDetail,
                    Data = 1
                };

                var count = 0;
                bool? success = null;

                //Starting PullRequests flow
                var flow = uiController.SelectFlow(UIControllerFlow.PullRequests);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Open a pull request
                            TriggerDetailViewOpen(uc, testPullRequestViewWithData);
                            break;
                        case 2:
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestDetailViewModel>>(uc);

                            //Completing a pull request
                            TriggerDone(uc);
                            break;
                        case 3:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Open a pull request
                            TriggerDetailViewOpen(uc, testPullRequestViewWithData);
                            break;
                        case 4:
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestDetailViewModel>>(uc);

                            //Cancelling on a pull request
                            TriggerCancel(uc);
                            break;
                        case 5:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Creating a pull request
                            TriggerCreationViewCreate(uc, null);
                            break;
                        case 6:
                            //Demonstrate view is IPullRequestCreationViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestCreationViewModel>>(uc);

                            //Cancelling on the creation of a pull request
                            TriggerCancel(uc);
                            break;
                        case 7:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Creating a pull request
                            TriggerCreationViewCreate(uc, null);
                            break;
                        case 8:
                            //Demonstrate view is IPullRequestCreationViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestCreationViewModel>>(uc);

                            //Completing a pull request
                            TriggerDone(uc);
                            break;
                        case 9:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Cancelling PullRequests flow
                            TriggerCancel(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.False(success);
            }
        }

        [Fact]
        public void Jump()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            //Simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 6;

                var testPullRequestViewWithData = new ViewWithData(UIControllerFlow.PullRequests)
                {
                    ViewType = GitHub.Exports.UIViewType.PRDetail,
                    Data = 1
                };

                var testPullRequestListViewWithData = new ViewWithData(UIControllerFlow.PullRequests)
                {
                    ViewType = GitHub.Exports.UIViewType.PRList,
                };

                var testPublishViewWithData = new ViewWithData(UIControllerFlow.Publish)
                {
                    ViewType = GitHub.Exports.UIViewType.Publish,
                };

                var count = 0;
                bool? success = null;

                //Starting PullRequests flow
                var flow = uiController.SelectFlow(UIControllerFlow.PullRequests);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);
                            
                            //Open a pull request
                            TriggerDetailViewOpen(uc, testPullRequestViewWithData);
                            break;
                        case 2:
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestDetailViewModel>>(uc);

                            //Attempt Jump to Publish View
                            Assert.Throws<ArgumentException>(()=> uiController.Jump(testPublishViewWithData));

                            //Demonstrate nothing happens when jump attempted to view outside of UIControllerFlow.PullRequests
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestDetailViewModel>>(uc);

                            //Jump to pull request list
                            uiController.Jump(testPullRequestListViewWithData);
                            break;
                        case 3:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Demonstrate jump to current view
                            uiController.Jump(testPullRequestListViewWithData);
                            break;
                        case 4:
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Jump back to pull request
                            uiController.Jump(testPullRequestViewWithData);
                            break;
                        case 5:
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestDetailViewModel>>(uc);

                            //Cancelling the pull request
                            TriggerCancel(uc);
                            break;
                        case 6:
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Cancelling PullRequests flow
                            TriggerCancel(uc);
                            break;
                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.False(success);
            }
        }

        [Fact]
        public void ShuttingDown()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            //Simulate being logged in
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost));

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 4;
                var testPullRequestViewWithData = new ViewWithData(UIControllerFlow.PullRequests)
                {
                    ViewType = GitHub.Exports.UIViewType.PRDetail,
                    Data = 1
                };

                var count = 0;
                bool? success = null;

                //Starting PullRequests flow
                var flow = uiController.SelectFlow(UIControllerFlow.PullRequests);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });
                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Open a pull request
                            TriggerDetailViewOpen(uc, testPullRequestViewWithData);
                            break;
                        case 2:
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestDetailViewModel>>(uc);

                            //Completing a pull request
                            TriggerDone(uc);
                            break;
                        case 3:
                            //Demonstrate view is IPullRequestListViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestListViewModel>>(uc);

                            //Open a pull request
                            TriggerDetailViewOpen(uc, testPullRequestViewWithData);
                            break;
                        case 4:
                            //Demonstrate view is IPullRequestDetailViewModel
                            Assert.IsAssignableFrom<IViewFor<IPullRequestDetailViewModel>>(uc);

                            //Stopping UIController
                            uiController.Stop();
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
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
            //Simulate being logged in without gist support
            const bool supportsGist = false;
            cons.Add(SetupConnection(provider, hosts, host, true, supportsGist));

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 3;

                var count = 0;
                bool? success = null;

                //Starting Gist flow
                var flow = uiController.SelectFlow(UIControllerFlow.Gist);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });

                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is ILogoutRequiredViewModel
                            Assert.IsAssignableFrom<IViewFor<ILogoutRequiredViewModel>>(uc);
                            host.IsLoggedIn.Returns(false);

                            //Completing View
                            TriggerDone(uc);
                            break;
                        case 2:
                            //Demonstrate view is ILoginControlViewModel
                            Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(uc);
                            
                            // login
                            host.IsLoggedIn.Returns(true);
                            host.SupportsGist.Returns(true);

                            //Completing View
                            TriggerDone(uc);
                            break;
                        case 3:
                            //Demonstrate view is IGistCreationViewModel
                            Assert.IsAssignableFrom<IViewFor<IGistCreationViewModel>>(uc);

                            //Completing PullRequests flow
                            TriggerDone(uc);
                            break;

                        default:
                            throw new Exception("Received more views than expected");
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
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

            //Simulate being logged in with gist support
            const bool supportsGist = true;
            cons.Add(SetupConnection(provider, hosts, hosts.GitHubHost, true, supportsGist));

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm))
            {
                const int testViewCount = 1;

                var count = 0;
                bool? success = null;

                //Starting Gist flow
                var flow = uiController.SelectFlow(UIControllerFlow.Gist);
                uiController.ListenToCompletionState()
                    .Subscribe(s =>
                    {
                        success = s;
                        Assert.Equal(testViewCount, count);
                        count++;
                    });

                flow.Subscribe(data =>
                {
                    var uc = data.View;
                    switch (++count)
                    {
                        case 1:
                            //Demonstrate view is IGistCreationViewModel
                            Assert.IsAssignableFrom<IViewFor<IGistCreationViewModel>>(uc);
                     
                            //Completing PullRequests flow
                            TriggerDone(uc);
                            break;

                        default:
                            Assert.True(false, "Received more views than expected");
                            break;
                    }
                }, () =>
                {
                    Assert.Equal(testViewCount + 1, count);
                    count++;
                });

                uiController.Start(null);
                Assert.Equal(testViewCount + 2, count);
                Assert.True(uiController.IsStopped);
                Assert.True(success.HasValue);
                Assert.True(success);
            }
        }
    }
}