using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System.Reflection;
using System.IO;
using Xunit;

namespace UnitTests.GitHub.VisualStudio
{
    public class AssemblyResolverPackageTests
    {
        public class TheResolveDependentAssemblyMethod
        {
            [Fact]
            public void ProvideCodeBaseAttribute_MatchFullName()
            {
                var asm = Assembly.GetExecutingAssembly();
                var assemblyName = asm.GetName().Name;
                var codeBase = $"$PackageFolder$\\{assemblyName}.dll";
                var provideCodeBase = new ProvideCodeBaseAttribute { AssemblyName = assemblyName, CodeBase = codeBase };
                var packageFolder = Path.GetDirectoryName(asm.Location);
                var resolveAssemblyName = new AssemblyName(asm.FullName);

                var resolvedAssembly = AssemblyResolverPackage.ResolveDependentAssembly(provideCodeBase, packageFolder, resolveAssemblyName);

                Assert.Equal(asm, resolvedAssembly);
            }

            [Fact]
            public void ProvideCodeBaseAttribute_DontMatchDifferentVersion()
            {
                var asm = Assembly.GetExecutingAssembly();
                var assemblyName = asm.GetName().Name;
                var codeBase = $"$PackageFolder$\\{assemblyName}.dll";
                var provideCodeBase = new ProvideCodeBaseAttribute { AssemblyName = assemblyName, CodeBase = codeBase };
                var packageFolder = Path.GetDirectoryName(asm.Location);
                var resolveAssemblyName = asm.GetName();
                resolveAssemblyName.Version = new Version("0.0.0.0");

                var resolvedAssembly = AssemblyResolverPackage.ResolveDependentAssembly(provideCodeBase, packageFolder, resolveAssemblyName);

                Assert.Null(resolvedAssembly);
            }

            [Fact]
            public void ProvideCodeBaseAttribute_DontMatchMissingCodeBase()
            {
                var asm = Assembly.GetExecutingAssembly();
                var assemblyName = asm.GetName().Name;
                var codeBase = "__NothingToSeeHere___";
                var provideCodeBase = new ProvideCodeBaseAttribute { AssemblyName = assemblyName, CodeBase = codeBase };
                var dependentAssemblies = new[] { provideCodeBase };
                var packageFolder = Path.GetDirectoryName(asm.Location);
                var resolveAssemblyName = new AssemblyName(asm.FullName);

                var resolvedAssembly = AssemblyResolverPackage.ResolveDependentAssembly(provideCodeBase, packageFolder, resolveAssemblyName);

                Assert.Null(resolvedAssembly);
            }

            [Fact]
            public void ProvideCodeBaseAttribute_DontMatchPartialName()
            {
                var asm = Assembly.GetExecutingAssembly();
                var assemblyName = asm.GetName().Name;
                var provideCodeBase = new ProvideCodeBaseAttribute { AssemblyName = assemblyName, CodeBase = $"$PackageFolder$\\{assemblyName}.dll" };
                var dependentAssemblies = new[] { provideCodeBase };
                var packageFolder = Path.GetDirectoryName(asm.Location);
                var resolveAssemblyName = new AssemblyName(asm.GetName().Name);

                var resolvedAssembly = AssemblyResolverPackage.ResolveDependentAssembly(provideCodeBase, packageFolder, resolveAssemblyName);

                Assert.Null(resolvedAssembly);
            }

            [Fact]
            public void ProvideBindingRedirectionAttribute_MatchOldVersionLowerBound()
            {
                var asm = Assembly.GetExecutingAssembly();
                var assemblyName = asm.GetName().Name;
                var codeBase = $"$PackageFolder$\\{assemblyName}.dll";
                var oldVersionLowerBound = "0.0.0.0";
                var oldVersionUpperBound = oldVersionLowerBound;
                var provideBindingRedirection = new ProvideBindingRedirectionAttribute
                {
                    AssemblyName = assemblyName,
                    CodeBase = codeBase,
                    OldVersionLowerBound = oldVersionLowerBound,
                    OldVersionUpperBound = oldVersionUpperBound
                };
                var dependentAssemblies = new[] { provideBindingRedirection };
                var packageFolder = Path.GetDirectoryName(asm.Location);
                var resolveAssemblyName = asm.GetName();
                resolveAssemblyName.Version = new Version(oldVersionLowerBound);

                var resolvedAssembly = AssemblyResolverPackage.ResolveDependentAssembly(provideBindingRedirection, packageFolder, resolveAssemblyName);

                Assert.Equal(asm, resolvedAssembly);
            }

            [Fact]
            public void ProvideBindingRedirectionAttribute_MatchOldVersionUpperBound()
            {
                var asm = Assembly.GetExecutingAssembly();
                var assemblyName = asm.GetName().Name;
                var codeBase = $"$PackageFolder$\\{assemblyName}.dll";
                var oldVersionUpperBound = "1.1.1.1";
                var oldVersionLowerBound = oldVersionUpperBound;
                var provideBindingRedirection = new ProvideBindingRedirectionAttribute
                {
                    AssemblyName = assemblyName,
                    CodeBase = codeBase,
                    OldVersionLowerBound = oldVersionLowerBound,
                    OldVersionUpperBound = oldVersionUpperBound
                };
                var dependentAssemblies = new[] { provideBindingRedirection };
                var packageFolder = Path.GetDirectoryName(asm.Location);
                var resolveAssemblyName = asm.GetName();
                resolveAssemblyName.Version = new Version(oldVersionLowerBound);

                var resolvedAssembly = AssemblyResolverPackage.ResolveDependentAssembly(provideBindingRedirection, packageFolder, resolveAssemblyName);

                Assert.Equal(asm, resolvedAssembly);
            }

            [Fact]
            public void ProvideBindingRedirectionAttribute_DontMatchOutOfBounds()
            {
                var asm = Assembly.GetExecutingAssembly();
                var assemblyName = asm.GetName().Name;
                var codeBase = $"$PackageFolder$\\{assemblyName}.dll";
                var oldVersionLowerBound = "0.0.0.0";
                var oldVersionUpperBound = "1.1.1.1";
                var resolveVersion = new Version("2.2.2.2");
                var provideBindingRedirection = new ProvideBindingRedirectionAttribute
                {
                    AssemblyName = assemblyName,
                    CodeBase = codeBase,
                    OldVersionLowerBound = oldVersionLowerBound,
                    OldVersionUpperBound = oldVersionUpperBound
                };
                var dependentAssemblies = new[] { provideBindingRedirection };
                var packageFolder = Path.GetDirectoryName(asm.Location);
                var resolveAssemblyName = asm.GetName();
                resolveAssemblyName.Version = resolveVersion;

                var resolvedAssembly = AssemblyResolverPackage.ResolveDependentAssembly(provideBindingRedirection, packageFolder, resolveAssemblyName);

                Assert.Null(resolvedAssembly);
            }
        }
    }
}
