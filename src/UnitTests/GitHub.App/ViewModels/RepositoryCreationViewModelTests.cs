using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Octokit;
using ReactiveUI;
using Rothko;
using Xunit;
using Xunit.Extensions;
using UnitTests;
using GitHub.Primitives;

public class RepositoryCreationViewModelTests
{
    static object DefaultInstance = new object();

    static IRepositoryCreationViewModel GetMeAViewModel(IServiceProvider provider = null)
    {
        if (provider == null)
            provider = Substitutes.ServiceProvider;
        var os = provider.GetOperatingSystem();
        var hosts = provider.GetRepositoryHosts();
        var creationService = provider.GetRepositoryCreationService();
        var cloneService = provider.GetRepositoryCloneService();
        var connection = provider.GetConnection();

        return new RepositoryCreationViewModel(connection, os, hosts, creationService, cloneService);
    }

    public class TheSafeRepositoryNameProperty
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

    public class TheBrowseForDirectoryCommand
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

    public class TheBaseRepositoryPathValidatorProperty
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
        public void ReturnsCorrectMessageWhenPathTooLong()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"C:\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Path too long", vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }
    }

    public class TheRepositoryNameValidatorProperty
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
            operatingSystem.File.Exists(@"c:\fake\foo\.git\HEAD").Returns(exists);
            var vm = GetMeAViewModel(provider);
            vm.BaseRepositoryPath = @"c:\fake\";

            vm.RepositoryName = "foo";

            Assert.Equal(expected, vm.RepositoryNameValidator.ValidationResult.IsValid);
            if (!expected)
                Assert.Equal("Repository with same name already exists at this location",
                    vm.RepositoryNameValidator.ValidationResult.Message);
        }
    }

    public class TheSafeRepositoryNameWarningValidatorProperty
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

    public class TheAccountsProperty
    {
        [Fact]
        public void IsPopulatedByTheRepositoryHost()
        {
            var provider = Substitutes.ServiceProvider;
            var hosts = provider.GetRepositoryHosts();
            var accounts = new ReactiveList<IAccount>() { Substitute.For<IAccount>() };
            hosts.GitHubHost.Accounts.Returns(accounts);
            var vm = GetMeAViewModel(provider);

            Assert.Same(accounts, vm.Accounts);
            Assert.Equal(vm.Accounts[0], vm.SelectedAccount);
        }
    }

    public class TheGitIgnoreTemplatesProperty
    {
        [Fact]
        public void IsPopulatedByTheApiAndSortedWithRecommendedFirstAndSelectsFirst()
        {
            var gitIgnoreTemplates = new[]
        {
            "Delphi",
            "VisualStudio",
            "Node",
            "Waf",
            "WordPress"
        };
            var provider = Substitutes.ServiceProvider;
            var hosts = provider.GetRepositoryHosts();
            var host = hosts.GitHubHost;
            hosts.LookupHost(Arg.Any<HostAddress>()).Returns(host);
            host.ApiClient
                .GetGitIgnoreTemplates()
                .Returns(gitIgnoreTemplates.ToObservable());
            var vm = GetMeAViewModel(provider);

            var result = vm.GitIgnoreTemplates;

            Assert.Equal(6, result.Count);
            Assert.Equal("None", result[0].Name);
            Assert.True(result[0].Recommended);
            Assert.Equal("VisualStudio", result[1].Name);
            Assert.True(result[1].Recommended);
            Assert.Equal("Node", result[2].Name);
            Assert.True(result[2].Recommended);
            Assert.Equal("Delphi", result[3].Name);
            Assert.False(result[3].Recommended);
            Assert.Equal("Waf", result[4].Name);
            Assert.False(result[4].Recommended);
            Assert.Equal("WordPress", result[5].Name);
            Assert.False(result[5].Recommended);
            Assert.Equal(result[0], vm.SelectedGitIgnoreTemplate);
        }
    }

    public class TheLicensesProperty
    {
        [Fact]
        public void IsPopulatedByTheApiAndSortedWithRecommendedFirst()
        {
            var licenses = new[]
        {
            new LicenseMetadata("agpl-3.0", "GNU Affero GPL v3.0", new Uri("https://whatever")),
            new LicenseMetadata("apache-2.0", "Apache License 2.0", new Uri("https://whatever")),
            new LicenseMetadata("artistic-2.0", "Artistic License 2.0", new Uri("https://whatever")),
            new LicenseMetadata("mit", "MIT License", new Uri("https://whatever"))
        };
            var provider = Substitutes.ServiceProvider;
            var hosts = provider.GetRepositoryHosts();
            var host = hosts.GitHubHost;
            hosts.LookupHost(Arg.Any<HostAddress>()).Returns(host);
            host.ApiClient
                .GetLicenses()
                .Returns(licenses.ToObservable());
            var vm = GetMeAViewModel(provider);

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

    public class TheCreateRepositoryCommand
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
            var hosts = provider.GetRepositoryHosts();
            hosts.GitHubHost.Accounts.Returns(new ReactiveList<IAccount> { account });
            var vm = GetMeAViewModel(provider);
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
            var hosts = provider.GetRepositoryHosts();
            hosts.GitHubHost.Accounts.Returns(new ReactiveList<IAccount> { account });
            var vm = GetMeAViewModel(provider);
            vm.RepositoryName = "Krieger";
            vm.BaseRepositoryPath = @"c:\dev";
            vm.SelectedAccount = account;
            vm.KeepPrivate = false;
            vm.SelectedLicense = new LicenseItem(new LicenseMetadata("mit", "MIT", new Uri("https://whatever")));

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
            var hosts = provider.GetRepositoryHosts();
            hosts.GitHubHost.Accounts.Returns(new ReactiveList<IAccount> { account });
            var vm = GetMeAViewModel(provider);
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
            var reactiveCommand = vm.CreateRepository as ReactiveCommand<Unit>;

            bool result = reactiveCommand.CanExecute(null);

            Assert.Equal(expected, result);
        }
    }

    public class TheCanKeepPrivateProperty
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
