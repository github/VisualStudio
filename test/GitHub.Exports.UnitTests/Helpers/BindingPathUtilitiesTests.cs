using System.IO;
using System.Reflection;
using System.Collections.Generic;
using GitHub.Helpers;
using NUnit.Framework;

public static class BindingPathUtilitiesTests
{
    public class TheFindAssemblyWithDifferentCodeBaseMethod
    {
        [Test]
        public void Current_Assembly_Has_No_Twin()
        {
            var assembly = GetType().Assembly;

            var foundAssembly = BindingPathUtilities.FindAssemblyWithDifferentCodeBase(assembly);

            Assert.That(foundAssembly, Is.Null);
        }

        [Test]
        public void Assembly_Loaded_From_Bytes_Has_Twin()
        {
            var expectAssembly = GetType().Assembly;
            var bytes = File.ReadAllBytes(expectAssembly.Location);
            var assembly = Assembly.Load(bytes);

            var foundAssembly = BindingPathUtilities.FindAssemblyWithDifferentCodeBase(assembly);

            Assert.That(foundAssembly, Is.EqualTo(expectAssembly));
        }
    }

    public class TheFindLoadedAssemblyWithSameName
    {
        [Test]
        public void Mscorlib_Has_No_Twin()
        {
            var assembly = typeof(object).Assembly;

            var foundAssembly = BindingPathUtilities.FindLoadedAssemblyWithSameName(assembly);

            Assert.That(foundAssembly, Is.Null);
        }

        [Test]
        public void Assembly_Loaded_From_Bytes_Has_Twin()
        {
            var expectAssembly = typeof(BindingPathUtilities).Assembly;
            var bytes = File.ReadAllBytes(expectAssembly.Location);
            var assembly = Assembly.Load(bytes);

            var foundAssembly = BindingPathUtilities.FindLoadedAssemblyWithSameName(assembly);

            Assert.That(foundAssembly, Is.EqualTo(expectAssembly));
        }
    }

    public class TheRationalizeBindingPathsMethod
    {
        [TestCase]
        public void Alternative_Path_Is_Removed_From_BinsingPaths()
        {
            var alternativeLocation = GetType().Assembly.Location;
            var fileName = Path.GetFileName(alternativeLocation);
            var alternativeDir = Path.GetDirectoryName(alternativeLocation);
            var assemblyDir = @"c:\target";
            var assemblyLocation = Path.Combine(assemblyDir, fileName);
            var bindingPaths = new List<string> { alternativeDir, assemblyDir };

            var removed = BindingPathUtilities.RationalizeBindingPaths(bindingPaths, assemblyLocation);

            Assert.That(bindingPaths, Contains.Item(assemblyDir));
            Assert.That(bindingPaths, Does.Not.Contain(alternativeDir));
            Assert.That(removed, Is.True);
        }

        [TestCase]
        public void Return_False_When_No_BindingPath_Was_Removed()
        {
            var assemblyLocation = GetType().Assembly.Location;
            var assemblyDir = Path.GetDirectoryName(assemblyLocation);
            var bindingPaths = new List<string> { assemblyDir };

            var removed = BindingPathUtilities.RationalizeBindingPaths(bindingPaths, assemblyLocation);

            Assert.That(removed, Is.False);
        }
    }

    public class TheFindBindingPathsMethod
    {
        [TestCase]
        public void Return_Empty_When_Not_In_Visual_Studio()
        {
            var bindingPaths = BindingPathUtilities.FindBindingPaths();

            Assert.IsEmpty(bindingPaths);
        }
    }

    public class TheIsAssemblyLoadedMethod
    {
        [TestCase]
        public void Check_Executing_Assumbly_Is_Loaded()
        {
            var location = GetType().Assembly.Location;

            var isLoaded = BindingPathUtilities.IsAssemblyLoaded(location);

            Assert.That(isLoaded, Is.True);
        }

        [TestCase]
        public void Check_Executing_Assumbly_Is_Loaded_When_Case_Differs()
        {
            var location = GetType().Assembly.Location.ToUpperInvariant();

            var isLoaded = BindingPathUtilities.IsAssemblyLoaded(location);

            Assert.That(isLoaded, Is.True);
        }

        [Test]
        public void Check_Assembly_At_Unknown_Location_Not_Loaded()
        {
            var location = @"c:\unknown.dll";

            var isLoaded = BindingPathUtilities.IsAssemblyLoaded(location);

            Assert.That(isLoaded, Is.False);
        }
    }
}
