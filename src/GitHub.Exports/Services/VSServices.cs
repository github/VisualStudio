using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GitHub.Extensions;
using Microsoft.Win32;

namespace GitHub.Services
{
    public interface IVSServices
    {
        string GetLocalClonePathFromGitProvider(IServiceProvider provider);
        void Clone(IServiceProvider provider, string cloneUrl, string clonePath, bool recurseSubmodules);
    }

    [Export(typeof(IVSServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSServices : IVSServices
    {
        static class GitCoreServices
        {
            const string ns = "Microsoft.TeamFoundation.Git.CoreServices.";
            const string scchost = "ISccServiceHost";
            const string sccservice = "ISccSettingsService";

            const string AssemblyName = "Microsoft.TeamFoundation.Git.CoreServices";
            static Assembly assembly;
            static Assembly Assembly
            {
                get
                {
                    if (assembly != null)
                        return assembly;
                    assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == AssemblyName);
                    Debug.Assert(assembly != null, string.Format(CultureInfo.InvariantCulture, "{0} is not loaded in the AppDomain. This might mean there's no git provider loaded yet.", AssemblyName));
                    return assembly;
                }
            }

            static Type iSccServiceHostType;
            static Type ISccServiceHostType
            {
                get
                {
                    if (iSccServiceHostType != null)
                        return iSccServiceHostType;
                    var asm = Assembly;
                    if (asm == null)
                        return null; // validation/debug.assert already done
                    iSccServiceHostType = asm.GetType(ns + scchost);
                    Debug.Assert(iSccServiceHostType != null, string.Format(CultureInfo.InvariantCulture, "'{0}' {1} not found in assembly '{2}'. We need to check if it's been moved in this version.",
                        ns + scchost, "type", asm.GetCustomAttributeValue<AssemblyFileVersionAttribute>("Version")));
                    return iSccServiceHostType;
                }
            }

            static Type iSccSettingsServiceType;
            static Type ISccSettingsServiceType
            {
                get
                {
                    if (iSccSettingsServiceType != null)
                        return iSccSettingsServiceType;
                    var asm = Assembly;
                    if (asm == null)
                        return null; // validation/debug.assert already done
                    iSccSettingsServiceType = asm.GetType(ns + sccservice);
                    Debug.Assert(iSccSettingsServiceType != null, string.Format(CultureInfo.InvariantCulture, "'{0}' {1} not found in assembly '{2}'. We need to check if it's been moved in this version.",
                        ns + sccservice, "type", asm.GetCustomAttributeValue<AssemblyFileVersionAttribute>("Version")));
                    return iSccSettingsServiceType;
                }
            }

            static IServiceProvider GetISccServiceHost(IServiceProvider provider)
            {
                var type = ISccServiceHostType;
                if (type == null)
                    return null; // validation/debug.assert already done
                var ret = provider.TryGetService(type) as IServiceProvider;
                Debug.Assert(ret != null, string.Format(CultureInfo.InvariantCulture, "'{0}' {1} not found in assembly '{2}'. We need to check if it's been moved in this version.",
                    type, "service", assembly.GetCustomAttributeValue<AssemblyFileVersionAttribute>("Version")));
                return ret;
            }

            public static ISccSettingsService GetISccSettingsService(IServiceProvider provider)
            {
                var type = ISccSettingsServiceType;
                if (type == null)
                    return null; // validation/debug.assert already done
                var host = GetISccServiceHost(provider);
                if (host == null)
                    return null; // validation/debug.assert already done
                var ret = host.TryGetService(type);
                Debug.Assert(ret != null, string.Format(CultureInfo.InvariantCulture, "'{0}' {1} not found in assembly '{2}'. We need to check if it's been moved in this version.",
                    type, "service", assembly.GetCustomAttributeValue<AssemblyFileVersionAttribute>("Version")));
                return new ISccSettingsService(ret);
            }

            public class ISccSettingsService
            {
                readonly object settings;
                public ISccSettingsService(object provider)
                {
                    settings = provider;
                }

                public string DefaultRepositoryPath
                {
                    get
                    {
                        return ISccSettingsServiceType.GetValueForProperty(settings, "DefaultRepositoryPath") as string;
                    }
                }
            }
        }

        // The Default Repository Path that VS uses is hidden in an internal
        // service 'ISccSettingsService' registered in an internal service
        // 'ISccServiceHost' in an assembly with no public types that's
        // always loaded with VS if the git service provider is loaded
        public string GetLocalClonePathFromGitProvider(IServiceProvider provider)
        {
            string ret = string.Empty;
            try
            {
                var service = GitCoreServices.GetISccSettingsService(provider);
                ret = service.DefaultRepositoryPath;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }

            if (!string.IsNullOrEmpty(ret))
                return ret;

            try
            {
                ret = PokeTheRegistryForLocalClonePath();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
            return ret;
        }

        public void Clone(IServiceProvider provider, string cloneUrl, string clonePath, bool recurseSubmodules)
        {
            var gitExt = provider.GetService<IGitRepositoriesExt>();
            gitExt.Clone(cloneUrl, clonePath, recurseSubmodules ? CloneOptions.RecurseSubmodule : CloneOptions.None);
        }

        static string PokeTheRegistryForLocalClonePath()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\14.0\TeamFoundation\GitSourceControl\General");
            return (string)key.GetValue("DefaultRepositoryPath", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);
        }
    }
}