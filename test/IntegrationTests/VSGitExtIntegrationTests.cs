using EnvDTE;
using EnvDTE80;
using GitHub.Services;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace IntegrationTests
{
    public class VSGitExtIntegrationTests
    {
        public class TheActiveRepositoriesProperty
        {
            readonly ITestOutputHelper output;

            public TheActiveRepositoriesProperty(ITestOutputHelper output)
            {
                this.output = output;
            }

            [VsFact(UIThread = true, Version = "2015-")]
            public async Task SolutionNotOnGit_NoActiveRepositoriesAsync()
            {
                var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                var componentModel = (IComponentModel)await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SComponentModel));
                var gitExt = componentModel.GetService<IVSGitExt>();

                dte.Solution.Open(@"C:\test\ClassLibraryNotInGit\ClassLibraryNotInGit.sln");
                Assert.True(await WaitForLocalPath(gitExt, null));
            }

            [VsFact(UIThread = true, Version = "2015-")]
            public async Task ClassLibraryInGit_HasActiveRepositoriesAsync()
            {
                var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                var componentModel = (IComponentModel)await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SComponentModel));
                var gitExt = componentModel.GetService<IVSGitExt>();

                dte.Solution.Open(@"C:\test\ClassLibraryInGit\ClassLibraryInGit.sln");
                Assert.True(await WaitForLocalPath(gitExt, @"C:\test\ClassLibraryInGit"));
            }

            [VsFact(UIThread = true, Version = "2015-")]
            public async Task OpenClassLibraryNotInGitThenClassLibraryInGit_ActiveRepositoriesChangesAsync()
            {
                var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                var componentModel = (IComponentModel)await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SComponentModel));
                var gitExt = componentModel.GetService<IVSGitExt>();

                dte.Solution.Open(@"C:\test\ClassLibraryNotInGit\ClassLibraryNotInGit.sln");
                Assert.True(await WaitForLocalPath(gitExt, null));

                dte.Solution.Open(@"C:\test\ClassLibraryInGit\ClassLibraryInGit.sln");
                Assert.True(await WaitForLocalPath(gitExt, @"C:\test\ClassLibraryInGit"));

                dte.Solution.Open(@"C:\test\ClassLibraryNotInGit\ClassLibraryNotInGit.sln");
                Assert.True(await WaitForLocalPath(gitExt, null));

                dte.Solution.Open(@"C:\test\ClassLibraryInGit\ClassLibraryInGit.sln");
                Assert.True(await WaitForLocalPath(gitExt, @"C:\test\ClassLibraryInGit"));
            }

            async Task AddSolutionToSourceControlAsync()
            {
                var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                dte.ExecuteCommand("Team.Git.AddSolutionToSourceControl");
                await Task.Yield();
            }

            async Task<Solution> CreateSolutionAsync()
            {
                var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                var solution = (Solution2)dte.Solution;
                var templatePath = solution.GetProjectTemplate("ClassLibrary.zip", "CSharp");
                var tempDir = Path.Combine(@"c:\test", Guid.NewGuid().ToString());
                var solutionPath = Path.Combine(tempDir, "test.sln");
                solution.Create(tempDir, "test");
                solution.AddFromTemplate(templatePath, tempDir, "MyClassLibrary", false);
                solution.SaveAs(solutionPath);
                output.WriteLine(solutionPath);
                await Task.Yield();
                return (Solution)solution;
            }

            Task<bool> WaitForLocalPath(IVSGitExt gitExt, string expectLocalPath)
            {
                return WaitFor(() =>
                {
                    var localPath = gitExt.ActiveRepositories.FirstOrDefault()?.LocalPath;
                    output.WriteLine($"found {localPath}, looking for {expectLocalPath}");
                    return localPath == expectLocalPath;
                });
            }

            async Task<bool> WaitFor(Func<bool> condition, int timeout = 20000, int delay = 100)
            {
                for (int count = 0; count < timeout; count += delay)
                {
                    if (condition())
                    {
                        return true;
                    }

                    await Task.Delay(delay);
                }

                return false;
            }
        }
    }
}
