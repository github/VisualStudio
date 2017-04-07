using System;
using System.IO;
using System.Windows;
using NUnit.Framework;

namespace GitHub.UI.Helpers.UnitTests
{
    public class LoadingResourceDictionaryIntegrationTests
    {
        public class TheSourceProperty
        {
            [Description("Load assembly using LoadFrom away from application base")]
            [TestCase(Urls.GitHub_UI_SharedDictionary_PackUrl)]
            [TestCase(Urls.GitHub_VisualStudio_UI_SharedDictionary_PackUrl)]
            [RequiresThread(System.Threading.ApartmentState.STA)]
            public void SetInLoadFromContext(string url)
            {
                var setup = new AppDomainSetup { ApplicationBase = "NOTHING_HERE" };
                AppDomainContext.Invoke(setup, () =>
                {
                    var target = new LoadingResourceDictionary();
                    var packUri = ResourceDictionaryUtilities.ToPackUri(url);

                    target.Source = packUri;

                    Assert.That(target.MergedDictionaries.Count, Is.GreaterThan(0));
                });
            }

            [Description("Load assembly using LoadFrom on application base")]
            [TestCase(Urls.GitHub_UI_SharedDictionary_PackUrl)]
            [TestCase(Urls.GitHub_VisualStudio_UI_SharedDictionary_PackUrl)]
            [RequiresThread(System.Threading.ApartmentState.STA)]
            public void SetInLoadContext(string url)
            {
                var target = new LoadingResourceDictionary();
                var packUri = ResourceDictionaryUtilities.ToPackUri(url);

                target.Source = packUri;

                Assert.That(target.MergedDictionaries.Count, Is.GreaterThan(0));
            }
        }

        public class TheResourceDictionarySourceProperty
        {
            [Description("This shows why LoadingResourceDictionary is necessary")]
            [TestCase(Urls.GitHub_UI_SharedDictionary_PackUrl)]
            [TestCase(Urls.GitHub_VisualStudio_UI_SharedDictionary_PackUrl)]
            public void SetInLoadFromContext(string url)
            {
                var setup = new AppDomainSetup { ApplicationBase = "NOTHING_HERE" };
                using (var context = new AppDomainContext(setup))
                {
                    var remote = context.CreateInstance<ResourceDictionaryContext>();

                    Assert.Throws<FileNotFoundException>(() => remote.CountResourceDictionaryAndSetSource(url));
                }
            }

            class ResourceDictionaryContext : MarshalByRefObject
            {
                internal void CountResourceDictionaryAndSetSource(string url)
                {
                    var target = new ResourceDictionary();
                    var packUri = ResourceDictionaryUtilities.ToPackUri(url);
                    target.Source = packUri;
                }
            }
        }
    }
}
