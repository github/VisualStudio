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
        static HashSet<string> dictionaries = new HashSet<string>();

        static SharedDictionaryManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyFromRunDir;
        }

        static Assembly LoadAssemblyFromRunDir(object sender, ResolveEventArgs e)
        {
            var name = new AssemblyName(e.Name);
            if (!dictionaries.Contains(name.Name))
                return null;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filename = Path.Combine(path, name.Name + ".dll");
            if (!File.Exists(filename))
                return null;
            return Assembly.LoadFrom(filename);
        }

        public static ResourceDictionary Load(string assemblyname, ResourceDictionary resources)
        {
            if (!dictionaries.Contains(assemblyname))
            {
                dictionaries.Add(assemblyname);
                Uri loc;
                //if (System.Reflection.Assembly.GetCallingAssembly().GetName().Name == assemblyname)
                //    loc = new Uri("/SharedDictionary.xaml", UriKind.Relative);
                //else
                    loc = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0};component/SharedDictionary.xaml", assemblyname), UriKind.RelativeOrAbsolute);
                var dic = (ResourceDictionary)Application.LoadComponent(loc);
                resources.MergedDictionaries.Add(dic);
                mergedDictionaries.MergedDictionaries.Add(dic);
            }
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
}
