using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

public class GitHubAssemblyTests
{
    [Theory]
    public void GitHub_Assembly_Should_Not_Reference_DesignTime_Assembly(string assemblyFile)
    {
        var asm = Assembly.LoadFrom(assemblyFile);
        foreach (var referencedAssembly in asm.GetReferencedAssemblies())
        {
            Assert.That(referencedAssembly.Name, Does.Not.EndWith(".DesignTime"),
                "DesignTime assemblies should be embedded not referenced");
        }
    }

    [Theory]
    public void GitHub_Assembly_Should_Not_Reference_System_Net_Http_Above_4_0(string assemblyFile)
    {
        var asm = Assembly.LoadFrom(assemblyFile);
        foreach (var referencedAssembly in asm.GetReferencedAssemblies())
        {
            if (referencedAssembly.Name == "System.Net.Http")
            {
                Assert.That(referencedAssembly.Version, Is.EqualTo(new Version("4.0.0.0")));
            }
        }
    }

    [DatapointSource]
    string[] GitHubAssemblies => Directory.GetFiles(AssemblyDirectory, "GitHub.*.dll");

    string AssemblyDirectory => Path.GetDirectoryName(GetType().Assembly.Location);
}
