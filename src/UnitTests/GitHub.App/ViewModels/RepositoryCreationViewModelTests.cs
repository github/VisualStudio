using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using NSubstitute;
using Octokit;
using Rothko;
using Xunit;

public class RepositoryCreationViewModelTests
{
    public class TheSafeRepositoryNameProperty
    {
        [Fact]
        public void IsTheSameAsTheRepositoryNameWhenTheInputIsSafe()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this-is-bad";

            Assert.Equal(vm.RepositoryName, vm.SafeRepositoryName);
        }

        [Fact]
        public void IsConvertedWhenTheRepositoryNameIsNotSafe()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.RepositoryName = "this is bad";

            Assert.Equal("this-is-bad", vm.SafeRepositoryName);
        }

        [Fact]
        public void IsNullWhenRepositoryNameIsNull()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());
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
            var windows = Substitute.For<IOperatingSystem>();
            windows.Dialog.BrowseForDirectory(@"c:\fake\dev", Args.String)
                .Returns(new BrowseDirectoryResult(@"c:\fake\foo"));
            var vm = new RepositoryCreationViewModel(windows, Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = @"c:\fake\dev";

            await vm.BrowseForDirectory.ExecuteAsync();

            Assert.Equal(@"c:\fake\foo", vm.BaseRepositoryPath);
        }

        [Fact]
        public async Task DoesNotChangeTheBaseRepositoryPathWhenUserDoesNotChooseResult()
        {
            var windows = Substitute.For<IOperatingSystem>();
            windows.Dialog.BrowseForDirectory(@"c:\fake\dev", Args.String)
                .Returns(BrowseDirectoryResult.Failed);
            var vm = new RepositoryCreationViewModel(windows, Substitute.For<IRepositoryHosts>());
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
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = "";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository path", vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenPathHasInvalidCharacters()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = @"c:\fake!!>\";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Path contains invalid characters",
                vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenLotsofInvalidCharactersInPath()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = @"c:\fake???\sajoisfaoia\afsofsafs::::\";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Path contains invalid characters",
                vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsValidWhenUserAccidentallyUsesForwardSlashes()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = @"c:\fake\sajoisfaoia/afsofsafs/";
            vm.RepositoryName = "foo";

            Assert.True(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
        }

        [Fact]
        public void IsFalseWhenPathIsNotRooted()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = "fake";
            vm.RepositoryName = "foo";

            Assert.False(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a valid path", vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenAfterBeingTrue()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());
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
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "thisisfine";

            Assert.True(vm.BaseRepositoryPathValidator.ValidationResult.IsValid);
            Assert.Empty(vm.BaseRepositoryPathValidator.ValidationResult.Message);
        }

        [Fact]
        public void ReturnsCorrectMessageWhenPathTooLong()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

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
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());
            vm.BaseRepositoryPath = @"c:\fake\";

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenAfterBeingTrue()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());
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
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.RepositoryName = "thisisfine";

            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);
        }
    }

    public class TheSafeRepositoryNameWarningValidatorProperty
    {
        [Fact]
        public void IsTrueWhenRepoNameIsSafe()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this-is-bad";

            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
        }

        [Fact]
        public void IsFalseWhenRepoNameIsNotSafe()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this is bad";

            Assert.False(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
            Assert.Equal("Will be created as this-is-bad", vm.SafeRepositoryNameWarningValidator.ValidationResult.Message);
        }
    }

    public class TheAccountsProperty
    {
        [Fact]
        public void StartsOffEmpty()
        {
            var vm = new RepositoryCreationViewModel(
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IRepositoryHosts>());
            Assert.Empty(vm.Accounts);
        }
    }

    public class TheGitIgnoreTemplatesProperty
    {
        [Fact]
        public void IsPopulatedByTheApiAndSortedWithRecommendedFirst()
        {
            var gitIgnoreTemplates = new[]
            {
                "Delphi",
                "VisualStudio",
                "Node",
                "Waf",
                "WordPress"
            };
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.ApiClient
                .GetGitIgnoreTemplates()
                .Returns(gitIgnoreTemplates.ToObservable());
            var vm = new RepositoryCreationViewModel(Substitute.For<IOperatingSystem>(), hosts);

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
                var hosts = Substitute.For<IRepositoryHosts>();
                hosts.GitHubHost.ApiClient
                    .GetLicenses()
                    .Returns(licenses.ToObservable());
                var vm = new RepositoryCreationViewModel(Substitute.For<IOperatingSystem>(), hosts);

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
            }
        }
    }
}