public class VSServicesTests
{
    public class TheCloneMethod : TestBaseClass
    {
        /*
        [Theory]
        [InlineData(true, CloneOptions.RecurseSubmodule)]
        [InlineData(false, CloneOptions.None)]
        public void CallsCloneOnVsProvidedCloneService(bool recurseSubmodules, CloneOptions expectedCloneOptions)
        {
            var provider = Substitute.For<IUIProvider>();
            var gitRepositoriesExt = Substitute.For<IGitRepositoriesExt>();
            provider.GetService(typeof(IGitRepositoriesExt)).Returns(gitRepositoriesExt);
            provider.TryGetService(typeof(IGitRepositoriesExt)).Returns(gitRepositoriesExt);
            var vsServices = new VSServices(provider);

            vsServices.Clone("https://github.com/github/visualstudio", @"c:\fake\ghfvs", recurseSubmodules);

            gitRepositoriesExt.Received()
                .Clone("https://github.com/github/visualstudio", @"c:\fake\ghfvs", expectedCloneOptions);
        }
        */
    }
}
