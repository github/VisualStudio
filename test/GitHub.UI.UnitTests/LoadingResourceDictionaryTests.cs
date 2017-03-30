using System;
using System.IO;
using System.Windows;
using NUnit.Framework;

namespace GitHub.UI.UnitTests
{
    public class LoadingResourceDictionaryTests
    {
        public class TheSourceProperty
        {
            [Description("Load assembly using LoadFrom away from application base")]
            [TestCase("pack://application:,,,/GitHub.UI;component/SharedDictionary.xaml")]
            [TestCase("pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
            [RequiresThread(System.Threading.ApartmentState.STA)]
            public void SetInLoadFromContext(string url)
            {
                var setup = new AppDomainSetup { ApplicationBase = "NOTHING_HERE" };
                using (var context = new AppDomainContext(setup))
                {
                    var remote = context.CreateInstance<LoadingResourceDictionaryContext>();

                    int count = remote.CountMergedDictionaries(url);

                    Assert.That(count, Is.GreaterThan(0));
                }
            }

            [Description("Load assembly using LoadFrom on application base")]
            [TestCase("pack://application:,,,/GitHub.UI;component/SharedDictionary.xaml")]
            [TestCase("pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
            [RequiresThread(System.Threading.ApartmentState.STA)]
            public void SetInLoadContext(string url)
            {
                var local = new LoadingResourceDictionaryContext();

                int count = local.CountMergedDictionaries(url);

                Assert.That(count, Is.GreaterThan(0));
            }

            class LoadingResourceDictionaryContext : MarshalByRefObject
            {
                internal int CountMergedDictionaries(string url)
                {
                    var target = new LoadingResourceDictionary();
                    var packUri = ResourceDictionaryUtilities.ToPackUri(url);

                    target.Source = packUri;

                    return target.MergedDictionaries.Count;
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
                    var packUri = ResourceDictionaryUtilities.ToPackUri(url);

                    target.Source = packUri;

                    return target.MergedDictionaries.Count;
                }
            }
        }
    }
}
