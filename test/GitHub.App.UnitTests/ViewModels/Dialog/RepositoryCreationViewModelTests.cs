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
using GitHub.ViewModels.Dialog;
using NSubstitute;
using Octokit;
using Rothko;
using UnitTests;
using NUnit.Framework;
using IConnection = GitHub.Models.IConnection;
using System.Windows.Input;
using System.Reactive.Concurrency;

public class RepositoryCreationViewModelTests
{
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

        var vm = new RepositoryCreationViewModel(factory, os, creationService, usageTracker);
        vm.InitializeAsync(connection).Wait();
        return vm;
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
        [Test]
        public void IsTheSameAsTheRepositoryNameWhenTheInputIsSafe()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this-is-bad";

            Assert.That(vm.RepositoryName, Is.EqualTo(vm.SafeRepositoryName));
        }

        [Test]
        public void IsConvertedWhenTheRepositoryNameIsNotSafe()
        {
            var vm = GetMeAViewModel();

            vm.RepositoryName = "this is bad";

            Assert.That("this-is-bad", Is.EqualTo(vm.SafeRepositoryName));
        }

        [Test]
        public void IsNullWhenRepositoryNameIsNull()
        {
            var vm = GetMeAViewModel();
            Assert.Null(vm.SafeRepositoryName);
            vm.RepositoryName = "not-null";
            vm.RepositoryName = null;

            Assert.That(vm.SafeRepositoryName, Is.Null);
        }
    }

    public class TheBrowseForDirectoryCommand : TestBaseClass
    {
        [Test]
        public async Task SetsTheBaseRepositoryPathWhenUserChoosesADirectoryAsync()
        {
            var provider = Substitutes.ServiceProvider;
            var windows = provider.GetOperatingSystem();
            windows.Dialog.BrowseForDirectory(@"c:\fake\dev", Args.String)
                .Returns(new BrowseDirectoryResult(@"c:\fake\foo"));
            var vm = GetMeAViewModel(provider);

            vm.BaseRepositoryPath = @"c:\fake\dev";

            await vm.BrowseForDirectory.Execute();

            Assert.That(@"c:\fake\foo", Is.EqualTo(vm.BaseRepositoryPath));
        }

        [Test]
        public async Task DoesNotChangeTheBaseRepositoryPathWhenUserDoesNotChooseResultAsync()
        {
            var provider = Substitutes.ServiceProvider;
            var windows = provider.GetOperatingSystem();
            windows.Dialog.BrowseForDirectory(@"c:\fake\dev", Args.String)
                .Returns(BrowseDirectoryResult.Failed);
            var vm = GetMeAViewModel(provider);
            vm.BaseRepositoryPath = @"c:\fake\dev";

            await vm.BrowseForDirectory.Execute();

            Assert.That(@"c:\fake\dev", Is.EqualTo(vm.BaseRepositoryPath));
        }
    }

    public class TheBaseRepositoryPathProperty : TestBaseClass
    {
        [Test]
        public void IsSetFromTheRepositoryCreationService()
        {
            var repositoryCreationService = Substitute.For<IRepositoryCreationService>();
            repositoryCreationService.DefaultClonePath.Returns(@"c:\fake\default");

            var vm = GetMeAViewModel(creationService: repositoryCreationService);

            Assert.That(@"c:\fake\default", Is.EqualTo(vm.BaseRepositoryPath));
        }
    }

    public class TheBaseRepositoryPathValidatorProperty : TestBaseClass
    {
        [Test]
        public void IsFalseWhenPathEmpty()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = "";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.That("Please enter a repository path", Is.EqualTo(vm.BaseRepositoryPathValidator.ValidationResult.Message));
        }

        [Test]
        public void IsFalseWhenPathHasInvalidCharacters()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake!!>\";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.That("Path contains invalid characters",
                Is.EqualTo(vm.BaseRepositoryPathValidator.ValidationResult.Message));
        }

        [Test]
        public void IsFalseWhenLotsofInvalidCharactersInPath()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake???\sajoisfaoia\afsofsafs::::\";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.That("Path contains invalid characters",
                Is.EqualTo(vm.BaseRepositoryPathValidator.ValidationResult.Message));
        }

        [Test]
        public void IsValidWhenUserAccidentallyUsesForwardSlashes()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\sajoisfaoia/afsofsafs/";
            vm.RepositoryName = "foo";

            Assert.True(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
        }

        [Test]
        public void IsFalseWhenPathIsNotRooted()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = "fake";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.That("Please enter a valid path", Is.EqualTo(vm.BaseRepositoryPathValidator.ValidationResult.Message));
        }

        [Test]
        public void IsFalseWhenAfterBeingTrue()
        {
            var vm = GetMeAViewModel();
            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "repo";

            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.That(vm.RepositoryNameValidator.ValidationResult.Message, Is.Empty);

            vm.BaseRepositoryPath = "";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.That("Please enter a repository path", Is.EqualTo(vm.BaseRepositoryPathValidator.ValidationResult.Message));
        }

        [Test]
        public void IsTrueWhenRepositoryNameAndPathIsValid()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "thisisfine";

            Assert.True(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.That(vm.BaseRepositoryPathValidator.ValidationResult.Message, Is.Empty);
        }

        [Test]
        public void IsTrueWhenSetToValidQuotedPath()
        {
            var vm = GetMeAViewModel();

            vm.RepositoryName = "thisisfine";
            vm.BaseRepositoryPath = @"""c:\fake""";

            Assert.True(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.That(@"c:\fake", Is.EqualTo(vm.BaseRepositoryPath));
        }

        [Test]
        public void ReturnsCorrectMessageWhenPathTooLong()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"C:\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.That("Path too long", Is.EqualTo(vm.BaseRepositoryPathValidator.ValidationResult.Message));
        }
    }

    public class TheRepositoryNameValidatorProperty : TestBaseClass
    {
        [Test]
        public void IsFalseWhenRepoNameEmpty()
        {
            var vm = GetMeAViewModel();
            vm.BaseRepositoryPath = @"c:\fake\";

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.That("Please enter a repository name", Is.EqualTo(vm.RepositoryNameValidator.ValidationResult.Message));
        }

        [Test]
        public void IsFalseWhenAfterBeingTrue()
        {
            var vm = GetMeAViewModel();
            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "repo";

            Assert.True(((ICommand)vm.CreateRepository).CanExecute(null));
            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.That(vm.RepositoryNameValidator.ValidationResult.Message, Is.Empty);

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.That("Please enter a repository name", Is.EqualTo(vm.RepositoryNameValidator.ValidationResult.Message));
        }

        [Test]
        public void IsTrueWhenRepositoryNameAndPathIsValid()
        {
            var vm = GetMeAViewModel();

            vm.RepositoryName = "thisisfine";

            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.That(vm.RepositoryNameValidator.ValidationResult.Message, Is.Empty);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void IsFalseWhenRepositoryAlreadyExists(bool exists, bool expected)
        {
            var provider = Substitutes.ServiceProvider;
            var operatingSystem = provider.GetOperatingSystem();
            operatingSystem.Directory.Exists(@"c:\fake\foo").Returns(exists);
            var vm = GetMeAViewModel(provider);
            vm.BaseRepositoryPath = @"c:\fake\";

            vm.RepositoryName = "foo";

            Assert.That(expected, Is.EqualTo(vm.RepositoryNameValidator.ValidationResult.IsValid));
            if (!expected)
                Assert.That("Repository with same name already exists at this location",
                    Is.EqualTo(vm.RepositoryNameValidator.ValidationResult.Message));
        }
    }

    public class TheSafeRepositoryNameWarningValidatorProperty : TestBaseClass
    {
        [Test]
        public void IsTrueWhenRepoNameIsSafe()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this-is-bad";

            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
        }

        [Test]
        public void IsFalseWhenRepoNameIsNotSafe()
        {
            var vm = GetMeAViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this is bad";

            Assert.False(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
            Assert.That("Will be created as this-is-bad", Is.EqualTo(vm.SafeRepositoryNameWarningValidator.ValidationResult.Message));
        }
    }

    public class TheAccountsProperty : TestBaseClass
    {
        [Test]
        public async Task IsPopulatedByTheRepositoryHosAsynct()
        {
            var accounts = new List<IAccount> { new AccountDesigner(), new AccountDesigner() };
            var connection = Substitute.For<IConnection>();
            var modelService = Substitute.For<IModelService>();
            connection.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            modelService.GetAccounts().Returns(Observable.Return(accounts));
            var vm = new RepositoryCreationViewModel(
                GetMeAFactory(modelService),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryCreationService>(),
                Substitute.For<IUsageTracker>());
            await vm.InitializeAsync(connection);

            Assert.That(vm.Accounts[0], Is.EqualTo(vm.SelectedAccount));
            Assert.That(2, Is.EqualTo(vm.Accounts.Count));
        }
    }

    public class TheGitIgnoreTemplatesProperty : TestBaseClass
    {
        [Test]
        public void IsPopulatedByTheApiAndSortedWithRecommendedFirstAsync()
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
            var result = vm.GitIgnoreTemplates;

            Assert.That(5, Is.EqualTo(result.Count));
            Assert.That("None", Is.EqualTo(result[0].Name));
            Assert.True(result[0].Recommended);
            Assert.That("Node", Is.EqualTo(result[1].Name));
            Assert.True(result[1].Recommended);
            Assert.That("VisualStudio", Is.EqualTo(result[2].Name));
            Assert.True(result[2].Recommended);
            Assert.That("Waf", Is.EqualTo(result[3].Name));
            Assert.False(result[3].Recommended);
            Assert.That("WordPress", Is.EqualTo(result[4].Name));
            Assert.False(result[4].Recommended);
        }
    }

    public class TheLicensesProperty : TestBaseClass
    {
        [Test]
        public void IsPopulatedByTheModelServiceAsync()
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

            var result = vm.Licenses;

            Assert.That(5, Is.EqualTo(result.Count));
            Assert.That("", Is.EqualTo(result[0].Key));
            Assert.That("None", Is.EqualTo(result[0].Name));
            Assert.True(result[0].Recommended);
            Assert.That("apache-2.0", Is.EqualTo(result[1].Key));
            Assert.True(result[1].Recommended);
            Assert.That("mit", Is.EqualTo(result[2].Key));
            Assert.True(result[2].Recommended);
            Assert.That("agpl-3.0", Is.EqualTo(result[3].Key));
            Assert.False(result[3].Recommended);
            Assert.That("artistic-2.0", Is.EqualTo(result[4].Key));
            Assert.False(result[4].Recommended);
            Assert.That(result[0], Is.EqualTo(vm.SelectedLicense));
        }
    }

    public class TheSelectedGitIgnoreProperty : TestBaseClass
    {
        [Test]
        public async Task DefaultsToVisualStudioAsync()
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

            Assert.That("VisualStudio", Is.EqualTo(vm.SelectedGitIgnoreTemplate.Name));
        }

        [Test]
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

            Assert.That("None", Is.EqualTo(vm.SelectedGitIgnoreTemplate.Name));
        }
    }

    public class TheCreateRepositoryCommand : TestBaseClass
    {
        [Test]
        public async Task DisplaysUserErrorWhenCreationFailsAsync()
        {
            var creationService = Substitutes.RepositoryCreationService;
            var provider = Substitutes.GetServiceProvider(creationService: creationService);

            creationService.CreateRepository(Args.NewRepository, Args.Account, Args.String, Args.ApiClient)
                .Returns(Observable.Throw<Unit>(new InvalidOperationException("Could not create a repository on GitHub")));
            var vm = GetMeAViewModel(provider);

            vm.RepositoryName = "my-repo";

            using (var handlers = ReactiveTestHelper.OverrideHandlersForTesting())
            {
                await vm.CreateRepository.Execute().Catch(Observable.Return(Unit.Default));

                Assert.That("Could not create a repository on GitHub", Is.EqualTo(handlers.LastError.ErrorMessage));
            }
        }

        [Test]
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

            vm.CreateRepository.Execute();

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

        [Test]
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

            vm.CreateRepository.Execute();

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

        [Test]
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

            vm.CreateRepository.Execute();

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

        [TestCase("", "", false)]
        [TestCase("", @"c:\dev", false)]
        [TestCase("blah", @"c:\|dev", false)]
        [TestCase("blah", @"c:\dev", true)]
        public void CannotCreateWhenRepositoryNameOrBasePathIsInvalid(
            string repositoryName,
            string baseRepositoryPath,
            bool expected)
        {
            var vm = GetMeAViewModel();
            vm.RepositoryName = repositoryName;
            vm.BaseRepositoryPath = baseRepositoryPath;

            bool result = ((ICommand)vm.CreateRepository).CanExecute(null);

            Assert.That(expected, Is.EqualTo(result));
        }
    }
}
