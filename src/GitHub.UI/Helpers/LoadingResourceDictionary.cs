using System;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace GitHub
{
    public class LoadingResourceDictionary : ResourceDictionary
    {
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        void EnsureAssemblyLoaded(Uri value)
        {
            try
            {
                var assemblyName = FindAssemblyNameFromPackUri(value);
                if (assemblyName == null)
                {
                    Trace.WriteLine($"Couldn't find assembly name in '{value}'");
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
                    Trace.WriteLine($"Couldn't find assembly at '{assemblyFile}'");
                    return;
                }

                var assembly = Assembly.LoadFrom(assemblyFile);
                assemblyDicts.Add(assemblyFile, assembly);
            }
            catch (Exception)
            {
                Trace.WriteLine($"Error loading assembly for '{value}");
            }
        }

        static string FindAssemblyNameFromPackUri(Uri packUri)
        {
            var path = packUri.LocalPath;
            if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var component = ";component/";
            int componentIndex = path.IndexOf(component, 1, StringComparison.OrdinalIgnoreCase);
            if (componentIndex == -1)
            {
                return null;
            }

            return path.Substring(1, componentIndex - 1);
        }
    }
}
