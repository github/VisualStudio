using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.SampleData;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Octokit;
using Rothko;
using UnitTests;
using Xunit;
using IConnection = GitHub.Models.IConnection;

public class RepositoryCreationViewModelTests
{
    static object DefaultInstance = new object();

    static IRepositoryCreationViewModel GetMeAViewModel(
        IServiceProvider provider = null,
        IRepositoryCreationService creationService = null,
        IModelService modelService = null)
    {
        if (provider == null)
            provider = Substitutes.ServiceProvider;
        var os = provider.GetOperatingSystem();
        creationService = creationService ?? provider.GetRepositoryCreationService();
        var avatarProvider = provider.GetAvatarProvider();
        var connection = provider.GetConnection();
        connection.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
        var usageTracker = Substitute.For<IUsageTracker>();
        modelService = modelService ?? Substitute.For<IModelService>();
        var factory = GetMeAFactory(modelService);

        return new RepositoryCreationViewModel(connection, factory, os, creationService, usageTracker);
    }

    static IModelServiceFactory GetMeAFactory(IModelService ms)
    {
        var result = Substitute.For<IModelServiceFactory>();
        result.CreateAsync(null).ReturnsForAnyArgs(ms);
        result.CreateBlocking(null).ReturnsForAnyArgs(ms);
        return result;
    }

    public class TheSafeRepositoryNameProperty : TestBaseClass
    {
        [Fact]
        public void IsTheSameAsTheRepositoryNameWhenTheInputIsSafe()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this-is-bad";

            Assert.Equal(vm.RepositoryName, vm.SafeRepositoryName);
        }

        [Fact]
        public void IsConvertedWhenTheRepositoryNameIsNotSafe()
        {
            var vm = GetMeAViewModel();

            vm.RepositoryName = "this is bad";

            Assert.Equal("this-is-bad", vm.SafeRepositoryName);
        }

        [Fact]
        public void IsNullWhenRepositoryNameIsNull()
        {
            var vm = GetMeAViewModel();
            Assert.Null(vm.SafeRepositoryName);
            vm.RepositoryName = "not-null";
            vm.RepositoryName = null;

            Assert.Null(vm.SafeRepositoryName);
        }
    }

    public class TheBrowseForDirectoryCommand : TestBaseClass
    {
        [Fact]
        public async Task SetsTheBaseRepositoryPathWhenUserChoosesADirectory()
        {
            var provider = Substitutes.ServiceProvider;
            var windows = provider.GetOperatingSystem();
            windows.Dialog.BrowseForDirectory(@"c:\fake\dev", Args.String)
                .Returns(new BrowseDirectoryResult(@"c:\fake\foo"));
            var vm = GetMeAViewModel(provider);

            vm.BaseRepositoryPath = @"c:\fake\dev";

            await vm.BrowseForDirectory.ExecuteAsync();

            Assert.Equal(@"c:\fake\foo", vm.BaseRepositoryPath);
        }

        [Fact]
        public async Task DoesNotChangeTheBaseRepositoryPathWhenUserDoesNotChooseResult()
        {
            var provider = Substitutes.ServiceProvider;
            var windows = provider.GetOperatingSystem();
            windows.Dialog.BrowseForDirectory(@"c:\fake\dev", Args.String)
                .Returns(BrowseDirectoryResult.Failed);
            var vm = GetMeAViewModel(provider);
            vm.BaseRepositoryPath = @"c:\fake\dev";

            await vm.BrowseForDirectory.ExecuteAsync();

            Assert.Equal(@"c:\fake\dev", vm.BaseRepositoryPath);
        }
    }

    public class TheBaseRepositoryPathProperty : TestBaseClass
    {
        [Fact]
        public void IsSetFromTheRepositoryCreationService()
        {
            var repositoryCreationService = Substitute.For<IRepositoryCreationService>();
            repositoryCreationService.DefaultClonePath.Returns(@"c:\fake\default");

            var vm = GetMeAViewModel(creationService: repositoryCreationService);

            Assert.Equal(@"c:\fake\default", vm.BaseRepositoryPath);
        }
    }

    public class TheBaseRepositoryPathValidatorProperty : TestBaseClass
    {
        [Fact]
        public void IsFalseWhenPathEmpty()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = "";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository path", vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenPathHasInvalidCharacters()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake!!>\";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Path contains invalid characters",
                vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenLotsofInvalidCharactersInPath()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake???\sajoisfaoia\afsofsafs::::\";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Path contains invalid characters",
                vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsValidWhenUserAccidentallyUsesForwardSlashes()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\sajoisfaoia/afsofsafs/";
            vm.RepositoryName = "foo";

            Assert.True(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
        }

        [Fact]
        public void IsFalseWhenPathIsNotRooted()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = "fake";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a valid path", vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenAfterBeingTrue()
        {
            var vm = GetMeAViewModel();
            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "repo";

            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);

            vm.BaseRepositoryPath = "";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository path", vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsTrueWhenRepositoryNameAndPathIsValid()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "thisisfine";

            Assert.True(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Empty(vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsTrueWhenSetToValidQuotedPath()
        {
            var vm = GetMeAViewModel();

            vm.RepositoryName = "thisisfine";
            vm.BaseRepositoryPath = @"""c:\fake""";

            Assert.True(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal(@"c:\fake", vm.BaseRepositoryPath);
        }

        [Fact]
        public void ReturnsCorrectMessageWhenPathTooLong()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"C:\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Path too long", vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }
    }

    public class TheRepositoryNameValidatorProperty : TestBaseClass
    {
        [Fact]
        public void IsFalseWhenRepoNameEmpty()
        {
            var vm = GetMeAViewModel();
            vm.BaseRepositoryPath = @"c:\fake\";

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenAfterBeingTrue()
        {
            var vm = GetMeAViewModel();
            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "repo";

            Assert.True(vm.CreateRepository.CanExecute(null));
            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsTrueWhenRepositoryNameAndPathIsValid()
        {
            var vm = GetMeAViewModel();

            vm.RepositoryName = "thisisfine";

            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void IsFalseWhenRepositoryAlreadyExists(bool exists, bool expected)
        {
            var provider = Substitutes.ServiceProvider;
            var operatingSystem = provider.GetOperatingSystem();
            operatingSystem.Directory.Exists(@"c:\fake\foo").Returns(exists);
            var vm = GetMeAViewModel(provider);
            vm.BaseRepositoryPath = @"c:\fake\";

            vm.RepositoryName = "foo";

            Assert.Equal(expected, vm.RepositoryNameValidator.ValidationResult.IsValid);
            if (!expected)
                Assert.Equal("Repository with same name already exists at this location",
                    vm.RepositoryNameValidator.ValidationResult.Message);
        }
    }

    public class TheSafeRepositoryNameWarningValidatorProperty : TestBaseClass
    {
        [Fact]
        public void IsTrueWhenRepoNameIsSafe()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this-is-bad";

            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
        }

        [Fact]
        public void IsFalseWhenRepoNameIsNotSafe()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this is bad";

            Assert.False(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
            Assert.Equal("Will be created as this-is-bad", vm.SafeRepositoryNameWarningValidator.ValidationResult.Message);
        }
    }

    public class TheAccountsProperty : TestBaseClass
    {
        [Fact]
        public void IsPopulatedByTheRepositoryHost()
        {
            var accounts = new List<IAccount> { new AccountDesigner(), new AccountDesigner() };
            var connection = Substitute.For<IConnection>();
            var modelService = Substitute.For<IModelService>();
            connection.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            modelService.GetAccounts().Returns(Observable.Return(accounts));
            var vm = new RepositoryCreationViewModel(
                connection,
                GetMeAFactory(modelService),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryCreationService>(),
                Substitute.For<IUsageTracker>());

            Assert.Equal(vm.Accounts[0], vm.SelectedAccount);
            Assert.Equal(2, vm.Accounts.Count);
        }
    }

    public class TheGitIgnoreTemplatesProperty : TestBaseClass
    {
        [Fact]
        public async void IsPopulatedByTheApiAndSortedWithRecommendedFirst()
        {
            var gitIgnoreTemplates = new[]
            {
                "VisualStudio",
                "Node",
                "Waf",
                "WordPress"
            }.Select(GitIgnoreItem.Create);
            var provider = Substitutes.ServiceProvider;
            var modelService = Substitute.For<IModelService>();
            modelService
                .GetGitIgnoreTemplates()
                .Returns(gitIgnoreTemplates.ToObservable());
            var vm = GetMeAViewModel(provider, modelService: modelService);

            // this is how long the default collection waits to process about 5 things with the default UI settings
            await Task.Delay(100);

            var result = vm.GitIgnoreTemplates;

            Assert.Equal(5, result.Count);
            Assert.Equal("None", result[0].Name);
            Assert.True(result[0].Recommended);
            Assert.Equal("VisualStudio", result[1].Name);
            Assert.True(result[1].Recommended);
            Assert.Equal("Node", result[2].Name);
            Assert.True(result[2].Recommended);
            Assert.Equal("Waf", result[3].Name);
            Assert.False(result[3].Recommended);
            Assert.Equal("WordPress", result[4].Name);
            Assert.False(result[4].Recommended);
        }
    }

    public class TheLicensesProperty : TestBaseClass
    {
        [Fact]
        public async void IsPopulatedByTheModelService()
        {
            var licenses = new[]
            {
                new LicenseItem("apache-2.0", "Apache License 2.0"),
                new LicenseItem("mit", "MIT License"),
                new LicenseItem("agpl-3.0", "GNU Affero GPL v3.0"),
                new LicenseItem("artistic-2.0", "Artistic License 2.0")
            };
            var provider = Substitutes.ServiceProvider;
            var modelService = Substitute.For<IModelService>();
            modelService
                .GetLicenses()
                .Returns(licenses.ToObservable());
            var vm = GetMeAViewModel(provider, modelService: modelService);

            // this is how long the default collection waits to process about 5 things with the default UI settings
            await Task.Delay(100);

            var result = vm.Licenses;

            Assert.Equal(5, result.Count);
            Assert.Equal("", result[0].Key);
            Assert.Equal("None", result[0].Name);
            Assert.True(result[0].Recommended);
            Assert.Equal("apache-2.0", result[1].Key);
            Assert.True(result[1].Recommended);
            Assert.Equal("mit", result[2].Key);
            Assert.True(result[2].Recommended);
            Assert.Equal("agpl-3.0", result[3].Key);
            Assert.False(result[3].Recommended);
            Assert.Equal("artistic-2.0", result[4].Key);
            Assert.False(result[4].Recommended);
            Assert.Equal(result[0], vm.SelectedLicense);
        }
    }

    public class TheSelectedGitIgnoreProperty : TestBaseClass
    {
        [Fact]
        public async void DefaultsToVisualStudio()
        {
            var gitignores = new[]
            {
                GitIgnoreItem.Create("C++"),
                GitIgnoreItem.Create("Node"),
                GitIgnoreItem.Create("VisualStudio"),
            };
            var provider = Substitutes.ServiceProvider;
            var modelService = Substitute.For<IModelService>();
            modelService
                .GetGitIgnoreTemplates()
                .Returns(gitignores.ToObservable());
            var vm = GetMeAViewModel(provider, modelService: modelService);

            // this is how long the default collection waits to process about 5 things with the default UI settings
            await Task.Delay(100);

            Assert.Equal("VisualStudio", vm.SelectedGitIgnoreTemplate.Name);
        }

        [Fact]
        public void DefaultsToNoneIfVisualStudioIsMissingSomehow()
        {
            var gitignores = new[]
            {
                GitIgnoreItem.None,
                GitIgnoreItem.Create("C++"),
                GitIgnoreItem.Create("Node"),
            };
            var provider = Substitutes.ServiceProvider;
            var modelService = Substitute.For<IModelService>();
            modelService
                .GetGitIgnoreTemplates()
                .Returns(gitignores.ToObservable());
            var vm = GetMeAViewModel(provider, modelService: modelService);

            Assert.Equal("None", vm.SelectedGitIgnoreTemplate.Name);
        }
    }

    public class TheCreateRepositoryCommand : TestBaseClass
    {
        [Fact]
        public async Task DisplaysUserErrorWhenCreationFails()
        {
            var creationService = Substitutes.RepositoryCreationService;
            var provider = Substitutes.GetServiceProvider(creationService: creationService);

            creationService.CreateRepository(Args.NewRepository, Args.Account, Args.String, Args.ApiClient)
                .Returns(Observable.Throw<Unit>(new InvalidOperationException("Could not create a repository on GitHub")));
            var vm = GetMeAViewModel(provider);

            vm.RepositoryName = "my-repo";

            using (var handlers = ReactiveTestHelper.OverrideHandlersForTesting())
            {
                await vm.CreateRepository.ExecuteAsync().Catch(Observable.Return(Unit.Default));

                Assert.Equal("Could not create a repository on GitHub", handlers.LastError.ErrorMessage);
            }
        }

        [Fact]
        public void CreatesARepositoryUsingTheCreationService()
        {
            var creationService = Substitutes.RepositoryCreationService;
            var provider = Substitutes.GetServiceProvider(creationService: creationService);

            var account = Substitute.For<IAccount>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetAccounts().Returns(Observable.Return(new List<IAccount> { account }));
            var vm = GetMeAViewModel(provider, modelService: modelService);
            vm.RepositoryName = "Krieger";
            vm.BaseRepositoryPath = @"c:\dev";
            vm.SelectedAccount = account;
            vm.KeepPrivate = true;

            vm.CreateRepository.Execute(null);

            creationService
                .Received()
                .CreateRepository(
                    Arg.Is<NewRepository>(r => r.Name == "Krieger"
                        && r.Private == true
                        && r.AutoInit == null
                        && r.LicenseTemplate == null
                        && r.GitignoreTemplate == null),
                    account,
                    @"c:\dev",
                    Args.ApiClient);
        }

        [Fact]
        public void SetsAutoInitToTrueWhenLicenseSelected()
        {
            var creationService = Substitutes.RepositoryCreationService;
            var provider = Substitutes.GetServiceProvider(creationService: creationService);
            var account = Substitute.For<IAccount>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetAccounts().Returns(Observable.Return(new List<IAccount> { account }));
            var vm = GetMeAViewModel(provider, modelService: modelService);
            vm.RepositoryName = "Krieger";
            vm.BaseRepositoryPath = @"c:\dev";
            vm.SelectedAccount = account;
            vm.KeepPrivate = false;
            vm.SelectedLicense = new LicenseItem("mit", "MIT");

            vm.CreateRepository.Execute(null);

            creationService
                .Received()
                .CreateRepository(
                    Arg.Is<NewRepository>(r => r.Name == "Krieger"
                        && r.Private == false
                        && r.AutoInit == true
                        && r.LicenseTemplate == "mit"
                        && r.GitignoreTemplate == null),
                    account,
                    @"c:\dev",
                    Args.ApiClient);
        }

        [Fact]
        public void SetsAutoInitToTrueWhenGitIgnore()
        {
            var creationService = Substitutes.RepositoryCreationService;
            var provider = Substitutes.GetServiceProvider(creationService: creationService);
            var account = Substitute.For<IAccount>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetAccounts().Returns(Observable.Return(new List<IAccount> { account }));
            var vm = GetMeAViewModel(provider, modelService: modelService);
            vm.RepositoryName = "Krieger";
            vm.BaseRepositoryPath = @"c:\dev";
            vm.SelectedAccount = account;
            vm.KeepPrivate = false;
            vm.SelectedGitIgnoreTemplate = GitIgnoreItem.Create("VisualStudio");

            vm.CreateRepository.Execute(null);

            creationService
                .Received()
                .CreateRepository(
                    Arg.Is<NewRepository>(r => r.Name == "Krieger"
                        && r.Private == false
                        && r.AutoInit == true
                        && r.LicenseTemplate == null
                        && r.GitignoreTemplate == "VisualStudio"),
                    account,
                    @"c:\dev",
                    Args.ApiClient);
        }

        [Theory]
        [InlineData("", "", false)]
        [InlineData("", @"c:\dev", false)]
        [InlineData("blah", @"c:\|dev", false)]
        [InlineData("blah", @"c:\dev", true)]
        public void CannotCreateWhenRepositoryNameOrBasePathIsInvalid(
            string repositoryName,
            string baseRepositoryPath,
            bool expected)
        {
            var vm = GetMeAViewModel();
            vm.RepositoryName = repositoryName;
            vm.BaseRepositoryPath = baseRepositoryPath;
            var reactiveCommand = vm.CreateRepository as ReactiveUI.ReactiveCommand<Unit>;

            bool result = reactiveCommand.CanExecute(null);

            Assert.Equal(expected, result);
        }
    }

    public class TheCanKeepPrivateProperty : TestBaseClass
    {
        [Theory]
        [InlineData(true, false, false, false)]
        [InlineData(true, false, true, false)]
        [InlineData(false, false, true, false)]
        [InlineData(true, true, true, true)]
        [InlineData(false, false, false, true)]
        public void IsOnlyTrueWhenUserIsEntepriseOrNotOnFreeAccountThatIsNotMaxedOut(
            bool isFreeAccount,
            bool isEnterprise,
            bool isMaxedOut,
            bool expected)
        {
            var selectedAccount = Substitute.For<IAccount>();
            selectedAccount.IsOnFreePlan.Returns(isFreeAccount);
            selectedAccount.IsEnterprise.Returns(isEnterprise);
            selectedAccount.HasMaximumPrivateRepositories.Returns(isMaxedOut);
            var vm = GetMeAViewModel();
            vm.SelectedAccount = selectedAccount;

            Assert.Equal(expected, vm.CanKeepPrivate);
        }
    }
}
