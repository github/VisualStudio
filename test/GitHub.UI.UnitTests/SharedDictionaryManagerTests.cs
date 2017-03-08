using System;
using System.IO;
using System.Windows;
using NUnit.Framework;
using GitHub.UI.Helpers;

namespace GitHub.UI.Tests
{
    public class SharedDictionaryManagerTests
    {
        public class TheSourceProperty
        {
            [TestCase("pack://application:,,,/GitHub.UI;component/SharedDictionary.xaml")]
            [TestCase("pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
            [RequiresThread(System.Threading.ApartmentState.STA)]
            public void SetSourceOnDifferentInstances_ExpectTheSameObjects(params string[] urls)
            {
                var setup = new AppDomainSetup { ApplicationBase = "NOTHING_HERE" };
                using (var context = new AppDomainContext(setup))
                {
                    var remote = context.CreateInstance<SharedDictionaryManagerContext>();
                    string expectDump = remote.DumpMergedDictionariesSharedDictionaryManager(urls);

                    var dump = remote.DumpMergedDictionariesSharedDictionaryManager(urls);

                    Assert.That(dump, Is.EqualTo(expectDump));
                }
            }

            // This is why we need `SharedDictionaryManager`.
            [TestCase("pack://application:,,,/GitHub.UI;component/SharedDictionary.xaml")]
            [TestCase("pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
            [RequiresThread(System.Threading.ApartmentState.STA)]
            public void SetResourceDictionarySourceOnDifferentInstances_ExpectDifferentObjects(params string[] urls)
            {
                var setup = new AppDomainSetup { ApplicationBase = "NOTHING_HERE" };
                using (var context = new AppDomainContext(setup))
                {
                    var remote = context.CreateInstance<SharedDictionaryManagerContext>();
                    string expectDump = remote.DumpMergedDictionariesLoadingResourceDictionary(urls);

                    var dump = remote.DumpMergedDictionariesLoadingResourceDictionary(urls);

                    Assert.That(dump, Is.Not.EqualTo(expectDump));
                }
            }

            class SharedDictionaryManagerContext : MarshalByRefObject
            {
                internal string DumpMergedDictionariesLoadingResourceDictionary(params string[] urls)
                {
                    var target = new LoadingResourceDictionary();
                    return DumpMergedDictionaries(target, urls);
                }

                internal string DumpMergedDictionariesSharedDictionaryManager(params string[] urls)
                {
                    var target = new SharedDictionaryManager();
                    return DumpMergedDictionaries(target, urls);
                }

                string DumpMergedDictionaries(ResourceDictionary target, params string[] urls)
                {
                    foreach(var url in urls)
                    {
                        SetProperty(target, "Source", ToPackUri(url));
                    }

                    return DumpResourceDictionary(target);
                }

                static void SetProperty(object target, string name, object value)
                {
                    var prop = target.GetType().GetProperty(name);
                    prop.SetValue(target, value);
                }
            }
        }

        public class TheResourceDictionarySourceProperty
        {
            [Description("This shows why LoadingResourceDictionary is necessary")]
            [TestCase("pack://application:,,,/GitHub.UI;component/SharedDictionary.xaml")]
            [TestCase("pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
            public void SetInLoadFromContext(string url)
            {
                var setup = new AppDomainSetup { ApplicationBase = "NOTHING_HERE" };
                using (var context = new AppDomainContext(setup))
                {
                    var remote = context.CreateInstance<ResourceDictionaryContext>();

                    Assert.Throws<FileNotFoundException>(() => remote.CountMergedDictionaries(url));
                }
            }

            class ResourceDictionaryContext : MarshalByRefObject
            {
                internal int CountMergedDictionaries(string url)
                {
                    var target = new ResourceDictionary();
                    var packUri = ToPackUri(url);

                    target.Source = packUri;

                    return target.MergedDictionaries.Count;
                }
            }
        }

        static Uri ToPackUri(string url)
        {
            if (!UriParser.IsKnownScheme("pack"))
            {
                // Register the pack scheme.
                new Application();
            }

            return new Uri(url);
        }

        class AppDomainContext : IDisposable
        {
            AppDomain domain;

            public AppDomainContext(AppDomainSetup setup)
            {
                var friendlyName = GetType().FullName;
                domain = AppDomain.CreateDomain(friendlyName, null, setup);
            }

            public T CreateInstance<T>()
            {
                return (T)domain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.CodeBase, typeof(T).FullName);
            }

            public void Dispose()
            {
                AppDomain.Unload(domain);
            }
        }

        static string DumpResourceDictionary(ResourceDictionary rd, string indent = "")
        {
            var writer = new StringWriter();
            DumpResourceDictionary(writer, rd);
            return writer.ToString();
        }

        static void DumpResourceDictionary(TextWriter writer, ResourceDictionary rd, string indent = "")
        {
            var source = rd.Source;
            if(source != null)
            {
                writer.WriteLine(indent + source + " (" + rd.GetType().FullName + ") # " + rd.GetHashCode());
                foreach (var child in rd.MergedDictionaries)
                {
                    DumpResourceDictionary(writer, child, indent + "  ");
                }
            }
            else
            {
                // ignore our empty nodes
                foreach (var child in rd.MergedDictionaries)
                {
                    DumpResourceDictionary(writer, child, indent);
                }
            }
        }
    }
}
