using System;
using NUnit.Framework;

namespace GitHub.UI.Helpers.UnitTests
{
    public partial class SharedDictionaryManagerIntegrationTests
    {
        public class TheSourceProperty
        {
            [TestCase(Urls.GitHub_UI_SharedDictionary_PackUrl)]
            [TestCase(Urls.GitHub_VisualStudio_UI_SharedDictionary_PackUrl)]
            [TestCase(Urls.GitHub_UI_SharedDictionary_FileUrl, Description = "This is a design time URL")]
            [TestCase(Urls.GitHub_VisualStudio_UI_SharedDictionary_FileUrl, Description = "This is a design time URL")]
            [RequiresThread(System.Threading.ApartmentState.STA)]
            public void SetSourceOnDifferentInstances_ExpectTheSameObjects(string url)
            {
                var setup = new AppDomainSetup { ApplicationBase = "NOTHING_HERE" };
                AppDomainContext.Invoke(setup, () =>
                {
                    var shared1 = new SharedDictionaryManager();
                    var expectDump = ResourceDictionaryUtilities.DumpMergedDictionaries(shared1, url);

                    var shared2 = new SharedDictionaryManager();
                    var dump = ResourceDictionaryUtilities.DumpMergedDictionaries(shared2, url);

                    Assert.That(dump, Is.EqualTo(expectDump));
                });
            }

            // This is why we need `SharedDictionaryManager`.
            [TestCase(Urls.GitHub_UI_SharedDictionary_PackUrl)]
            [TestCase(Urls.GitHub_VisualStudio_UI_SharedDictionary_PackUrl)]
            [RequiresThread(System.Threading.ApartmentState.STA)]
            public void SetResourceDictionarySourceOnDifferentInstances_ExpectDifferentObjects(string url)
            {
                var setup = new AppDomainSetup { ApplicationBase = "NOTHING_HERE" };
                AppDomainContext.Invoke(setup, () =>
                {
                    var shared = new SharedDictionaryManager();
                    var expectDump = ResourceDictionaryUtilities.DumpMergedDictionaries(shared, url);

                    var loading = new LoadingResourceDictionary();
                    var dump = ResourceDictionaryUtilities.DumpMergedDictionaries(loading, url);

                    Assert.That(dump, Is.Not.EqualTo(expectDump));
                });
            }

            class SharedDictionaryManagerContext : MarshalByRefObject
            {
                internal string DumpMergedDictionariesLoadingResourceDictionary(string url)
                {
                    var target = new LoadingResourceDictionary();
                    return ResourceDictionaryUtilities.DumpMergedDictionaries(target, url);
                }

                internal string DumpMergedDictionariesSharedDictionaryManager(string url)
                {
                    var target = new SharedDictionaryManager();
                    return ResourceDictionaryUtilities.DumpMergedDictionaries(target, url);
                }
            }
        }
    }
}
