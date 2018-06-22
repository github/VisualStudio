using System;
using System.IO;
using System.IO.Packaging;
using System.Windows;

namespace GitHub.UI.Helpers.UnitTests
{
    class ResourceDictionaryUtilities
    {
        public static string PackUriScheme { get; private set; }

        public static Uri ToPackUri(string url)
        {
            // Calling `Application.Current` will install pack URI scheme via Application.cctor.
            // This is needed when unit testing for the pack:// URL format to be understood.
            if (Application.Current != null) { }

            return new Uri(url);
        }

        public static string DumpMergedDictionaries(ResourceDictionary target, string url)
        {
            SetProperty(target, "Source", ToPackUri(url));
            return DumpResourceDictionary(target);
        }

        static void SetProperty(object target, string name, object value)
        {
            var prop = target.GetType().GetProperty(name);
            prop.SetValue(target, value);
        }

        public static string DumpResourceDictionary(ResourceDictionary rd, string indent = "")
        {
            var writer = new StringWriter();
            DumpResourceDictionary(writer, rd);
            return writer.ToString();
        }

        public static void DumpResourceDictionary(TextWriter writer, ResourceDictionary rd, string indent = "")
        {
            var source = rd.Source;
            if (source != null)
            {
                writer.WriteLine(indent + source + " (" + rd.GetType().FullName + ") # " + rd.GetHashCode());
                indent += "  ";
            }

            foreach (var child in rd.MergedDictionaries)
            {
                DumpResourceDictionary(writer, child, indent);
            }
        }
    }
}
