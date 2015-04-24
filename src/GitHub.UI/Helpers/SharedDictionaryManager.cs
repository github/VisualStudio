using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Reflection;
using System.IO;

namespace GitHub.UI.Helpers
{
    public static class SharedDictionaryManager
    {
        static ResourceDictionary mergedDictionaries = new ResourceDictionary();
        static Dictionary<string, ResourceDictionary> dictionaries = new Dictionary<string, ResourceDictionary>();

        static SharedDictionaryManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyFromRunDir;
        }

        static Assembly LoadAssemblyFromRunDir(object sender, ResolveEventArgs e)
        {
            var name = new AssemblyName(e.Name);
            if (!dictionaries.ContainsKey(name.Name))
                return null;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filename = Path.Combine(path, name.Name + ".dll");
            if (!File.Exists(filename))
                return null;
            return Assembly.LoadFrom(filename);
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
    }

    public class CachedResourceDictionary : ResourceDictionary
    {
        static Dictionary<Uri, ResourceDictionary> dictionaries = new Dictionary<Uri, ResourceDictionary>();

        Uri sourceUri;
        public new Uri Source
        {
            get { return sourceUri; }
            set
            {
                sourceUri = value;
                ResourceDictionary ret;
                if (dictionaries.TryGetValue(value, out ret))
                {
                    MergedDictionaries.Add(ret);
                    return;
                }
                base.Source = value;
                dictionaries.Add(value, this);
            }
        }
    }
}
