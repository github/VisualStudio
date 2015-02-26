using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GitHub.UI.Helpers
{
    public static class SharedDictionaryManager
    {
        static ResourceDictionary mergedDictionaries = new ResourceDictionary();
        static HashSet<string> dictionaries = new HashSet<string>();

        public static ResourceDictionary Load(string assemblyname)
        {
            if (!dictionaries.Contains(assemblyname))
            {
                Uri loc;
                //if (System.Reflection.Assembly.GetCallingAssembly().GetName().Name == assemblyname)
                //    loc = new Uri("/SharedDictionary.xaml", UriKind.Relative);
                //else
                    loc = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0};component/SharedDictionary.xaml", assemblyname), UriKind.Relative);
                dictionaries.Add(assemblyname);
                var dic = (ResourceDictionary)Application.LoadComponent(loc);
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
