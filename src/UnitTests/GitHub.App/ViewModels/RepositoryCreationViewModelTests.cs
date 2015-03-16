using GitHub.ViewModels;
using Xunit;

public class RepositoryCreationViewModelTests
{
    public class TheSafeRepositoryNameProperty
    {
        [Fact]
        public void IsTheSameAsTheRepositoryNameWhenTheInputIsSafe()
        {
            var vm = new RepositoryCreationViewModel();

            vm.BaseRepositoryPath = @"c:\fake\";
            vm.RepositoryName = "this-is-bad";

            Assert.Equal(vm.RepositoryName, vm.SafeRepositoryName);
        }

        [Fact]
        public void IsConvertedWhenTheRepositoryNameIsNotSafe()
        {
            var vm = new RepositoryCreationViewModel();

            vm.RepositoryName = "this is bad";

            Assert.Equal("this-is-bad", vm.SafeRepositoryName);
        }

        [Fact]
        public void IsNullWhenRepositoryNameIsNull()
        {
            var vm = new RepositoryCreationViewModel();
            Assert.Null(vm.SafeRepositoryName);
            vm.RepositoryName = "not-null";
            vm.RepositoryName = null;

            Assert.Null(vm.SafeRepositoryName);
        }
    }
}
