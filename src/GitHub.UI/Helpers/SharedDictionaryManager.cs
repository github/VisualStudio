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
}
