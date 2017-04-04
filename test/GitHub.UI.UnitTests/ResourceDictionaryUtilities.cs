using System;
using System.IO;
using System.Windows;

namespace GitHub.UI.UnitTests
{
    class ResourceDictionaryUtilities
    {
        public static Uri ToPackUri(string url)
        {
            if (!UriParser.IsKnownScheme("pack"))
            {
                // Register the pack scheme.
                new Application();
            }

            return new Uri(url);
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
