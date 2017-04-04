using System;
using System.Windows;
using NUnit.Framework;
using GitHub.UI.Helpers;

namespace GitHub.UI.UnitTests
{
    public partial class SharedDictionaryManagerTests
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
                        SetProperty(target, "Source", ResourceDictionaryUtilities.ToPackUri(url));
                    }

                    return ResourceDictionaryUtilities.DumpResourceDictionary(target);
                }

                static void SetProperty(object target, string name, object value)
                {
                    var prop = target.GetType().GetProperty(name);
                    prop.SetValue(target, value);
                }
            }
        }
    }
}
