using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Xunit;

public class RepositoryCloneViewModelTests
{
    public class TheCloneCommand
    {
        [Fact]
        public void IsEnabledWhenRepositorySelected()
        {
            var repositoryHost = Substitute.For<IRepositoryHost>();
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var avatarProvider = Substitute.For<IAvatarProvider>();
            var vm = new RepositoryCloneViewModel(repositoryHost, cloneService, avatarProvider);

            Assert.False(vm.CloneCommand.CanExecute(null));

            vm.SelectedRepository = Substitute.For<IRepositoryModel>();

            Assert.True(vm.CloneCommand.CanExecute(null));
        }
    }
}
