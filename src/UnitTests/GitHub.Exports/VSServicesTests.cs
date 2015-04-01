using System;
using GitHub.Services;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using NSubstitute;
using Xunit;

public class VSServicesTests
{
    public class TheCloneMethod
    {
        [Theory]
        [InlineData(true, CloneOptions.RecurseSubmodule)]
        [InlineData(false, CloneOptions.None)]
        public void CallsCloneOnVsProvidedCloneService(bool recurseSubmodules, CloneOptions expectedCloneOptions)
        {
            var vsServices = new VSServices();
            var gitRepositoriesExt = Substitute.For<IGitRepositoriesExt>();
            var provider = Substitute.For<IServiceProvider>();
            provider.GetService(typeof(IGitRepositoriesExt)).Returns(gitRepositoriesExt);

            vsServices.Clone(provider, "https://github.com/github/visualstudio", @"c:\fake\ghfvs", recurseSubmodules);

            gitRepositoriesExt.Received()
                .Clone("https://github.com/github/visualstudio", @"c:\fake\ghfvs", expectedCloneOptions);
        }
    }
}
