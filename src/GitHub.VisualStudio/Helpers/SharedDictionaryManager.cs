using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Reflection;
using System.IO;
using System.Linq;

namespace GitHub.VisualStudio.Helpers
{
    public class SharedDictionaryManager : ResourceDictionary
    {
        static string[] ourAssemblies =
        {
            "GitHub.Api",
            "GitHub.App",
            "GitHub.CredentialManagement",
            "GitHub.Exports",
            "GitHub.Exports.Reactive",
            "GitHub.Extensions",
            "GitHub.Extensions.Reactive",
            "GitHub.UI",
            "GitHub.UI.Reactive",
            "GitHub.VisualStudio"
        };

        static ResourceDictionary mergedDictionaries = new ResourceDictionary();
        static Dictionary<string, ResourceDictionary> dictionaries = new Dictionary<string, ResourceDictionary>();

        static SharedDictionaryManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyFromRunDir;
        }

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        static Assembly LoadAssemblyFromRunDir(object sender, ResolveEventArgs e)
        {
            try
            {
                var name = new AssemblyName(e.Name);
                if (!ourAssemblies.Contains(name.Name))
                    return null;
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filename = Path.Combine(path, name.Name + ".dll");
                if (!File.Exists(filename))
                    return null;
                return Assembly.LoadFrom(filename);
            }
            catch (Exception ex)
            {
                var log = string.Format(CultureInfo.CurrentCulture, "Error occurred loading {0} from {1}.{2}{3}{4}", e.Name, Assembly.GetExecutingAssembly().Location, Environment.NewLine, ex, Environment.NewLine);
                VsOutputLogger.Write(log);
            }
            return null;
        }

        public static ResourceDictionary Load(string assemblyname, ResourceDictionary resources)
        {
            ResourceDictionary dic;
            if (!dictionaries.ContainsKey(assemblyname))
            {
                var loc = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0};component/SharedDictionary.xaml", assemblyname), UriKind.RelativeOrAbsolute);
                dic = (ResourceDictionary)Application.LoadComponent(loc);
                dictionaries.Add(assemblyname, dic);
            }
            else
                dic = dictionaries[assemblyname];
            resources.MergedDictionaries.Add(dic);
            return mergedDictionaries;
        }

        public static ResourceDictionary SharedDictionary
        {
            get
            {
                return mergedDictionaries;
            }
        }

#region ResourceDictionaryImplementation
        static Dictionary<Uri, ResourceDictionary> resourceDicts = new Dictionary<Uri, ResourceDictionary>();

        Uri sourceUri;
        public new Uri Source
        {
            get { return sourceUri; }
            set
            {
                sourceUri = value;
                ResourceDictionary ret;
                if (resourceDicts.TryGetValue(value, out ret))
                {
                    MergedDictionaries.Add(ret);
                    return;
                }
                base.Source = value;
                resourceDicts.Add(value, this);
            }
        }
#endregion
    }
}