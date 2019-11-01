using System.IO;
using System.Collections.Generic;
using GitHub.VisualStudio.Helpers;
using NUnit.Framework;

public static class BindingPathHelperTests
{
    public class TheFindRedundantBindingPathsMethod
    {
        [TestCase]
        public void Redundant_Binding_Paths_Contains_Alternative_Path()
        {
            var alternativeLocation = GetType().Assembly.Location;
            var fileName = Path.GetFileName(alternativeLocation);
            var alternativeDir = Path.GetDirectoryName(alternativeLocation);
            var assemblyDir = @"c:\target";
            var assemblyLocation = Path.Combine(assemblyDir, fileName);
            var bindingPaths = new List<string> { alternativeDir, assemblyDir };

            var paths = BindingPathHelper.FindRedundantBindingPaths(bindingPaths, assemblyLocation);

            Assert.That(paths, Contains.Item(alternativeDir));
            Assert.That(paths, Does.Not.Contain(assemblyDir));
        }

        [TestCase]
        public void No_Redundant_Binding_Paths()
        {
            var assemblyLocation = GetType().Assembly.Location;
            var assemblyDir = Path.GetDirectoryName(assemblyLocation);
            var bindingPaths = new List<string> { assemblyDir };

            var paths = BindingPathHelper.FindRedundantBindingPaths(bindingPaths, assemblyLocation);

            Assert.That(paths, Does.Not.Contain(assemblyDir));
        }
    }
}
