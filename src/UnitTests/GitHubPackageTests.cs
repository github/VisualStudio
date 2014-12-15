using System;
using System.ComponentModel.Design;
using GitHub.VisualStudio;
using NSubstitute;
using Xunit;

public class GitHubPackageTests
{
    public class TheInitializeMethod
    {
        public void AddsTopLevelMenuItems()
        {
            var menuService = new FakeMenuCommandService();
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IMenuCommandService)).Returns(menuService);
            var package = new GitHubPackageTestWrapper(serviceProvider);
            
            package.CallInitialize();

            Assert.Equal(2, menuService.AddedCommands.Count);
        }
    }

    public class GitHubPackageTestWrapper : GitHubPackage
    {
        public GitHubPackageTestWrapper(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void CallInitialize()
        {
            Initialize();
        }
    }
}
