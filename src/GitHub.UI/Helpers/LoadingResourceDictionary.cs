using System;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Collections.Generic;
using GitHub.VisualStudio;

namespace GitHub
{
    public class LoadingResourceDictionary : ResourceDictionary
    {
#if !XAML_DESIGNER // XAML Designer doesn't work if `Source` property has been replaced.
        static Dictionary<string, Assembly> assemblyDicts = new Dictionary<string, Assembly>();

        public new Uri Source
        {
            get { return base.Source; }
            set
            {
                EnsureAssemblyLoaded(value);
                base.Source = value;
            }
        }

        void EnsureAssemblyLoaded(Uri value)
        {
            try
            {
                var assemblyName = FindAssemblyNameFromPackUri(value);
                if (assemblyName == null)
                {
                    VsOutputLogger.WriteLine($"Couldn't find assembly name in '{value}'.");
                    return;
                }

                var baseDir = Path.GetDirectoryName(GetType().Assembly.Location);
                var assemblyFile = Path.Combine(baseDir, assemblyName + ".dll");
                if (assemblyDicts.ContainsKey(assemblyFile))
                {
                    return;
                }

                if (!File.Exists(assemblyFile))
                {
                    VsOutputLogger.WriteLine($"Couldn't find assembly at '{assemblyFile}'.");
                    return;
                }

                var assembly = Assembly.LoadFrom(assemblyFile);
                assemblyDicts.Add(assemblyFile, assembly);
            }
            catch(Exception e)
            {
                VsOutputLogger.WriteLine($"Error loading assembly for '{value}': {e}");
            }
        }

        static string FindAssemblyNameFromPackUri(Uri packUri)
        {
            var path = packUri.LocalPath;
            if (!path.StartsWith("/"))
            {
                return null;
            }

            var component = ";component/";
            int componentIndex = path.IndexOf(component, 1);
            if (componentIndex == -1)
            {
                return null;
            }

            return path.Substring(1, componentIndex - 1);
        }
#endif
    }
}
