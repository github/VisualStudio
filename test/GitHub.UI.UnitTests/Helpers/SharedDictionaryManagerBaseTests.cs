using System;
using System.IO;
using System.Windows;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.UI.Helpers.UnitTests
{
    public class SharedDictionaryManagerBaseTests
    {
        public class TheCachingFactoryClass
        {
            public class TheGetOrCreateResourceDictionaryMethod
            {
                [Test]
                public void ReturnsResourceDictionary()
                {
                    using (var factory = new SharedDictionaryManagerBase.CachingFactory())
                    {
                        var uri = ResourceDictionaryUtilities.ToPackUri("pack://application:,,,/GitHub.UI.UnitTests;component/Helpers/SharedDictionary.xaml");
                        var owner = new ResourceDictionary();

                        var resourceDictionary = factory.GetOrCreateResourceDictionary(owner, uri);

                        Assert.That(resourceDictionary, Is.Not.Null);
                    }
                }

                [Test]
                public void ReturnsCachedResourceDictionary()
                {
                    using (var factory = new SharedDictionaryManagerBase.CachingFactory())
                    {
                        var uri = ResourceDictionaryUtilities.ToPackUri("pack://application:,,,/GitHub.UI.UnitTests;component/Helpers/SharedDictionary.xaml");
                        var owner = new ResourceDictionary();

                        var resourceDictionary1 = factory.GetOrCreateResourceDictionary(owner, uri);
                        var resourceDictionary2 = factory.GetOrCreateResourceDictionary(owner, uri);

                        Assert.That(resourceDictionary1, Is.EqualTo(resourceDictionary2));
                    }
                }
            }

            public class TheDisposeMethod
            {
                [Test]
                public void CallsDisposeOnDisposable()
                {
                    using (var factory = new SharedDictionaryManagerBase.CachingFactory())
                    {
                        var disposable = Substitute.For<IDisposable>();
                        factory.TryAddDisposable(disposable);

                        factory.Dispose();

                        disposable.Received(1).Dispose();
                    }
                }

                [Test]
                public void AddedTwice_DisposeCalledOnce()
                {
                    using (var factory = new SharedDictionaryManagerBase.CachingFactory())
                    {
                        var disposable = Substitute.For<IDisposable>();
                        factory.TryAddDisposable(disposable);
                        factory.TryAddDisposable(disposable);

                        factory.Dispose();

                        disposable.Received(1).Dispose();
                    }
                }
            }
        }

        public class TheGetCurrentDomainCachingFactoryMethod
        {
            [Test]
            public void CalledTwice_DisposeNotCalled()
            {
                using (var factory = SharedDictionaryManagerBase.GetCurrentDomainCachingFactory())
                {
                    var disposable = Substitute.For<IDisposable>();
                    factory.TryAddDisposable(disposable);

                    SharedDictionaryManagerBase.GetCurrentDomainCachingFactory();

                    disposable.Received(0).Dispose();
                }
            }

            [Test]
            public void InvokeMethodOnNewAssembly_DisposeCalled()
            {
                using (var factory = SharedDictionaryManagerBase.GetCurrentDomainCachingFactory())
                {
                    var disposable = Substitute.For<IDisposable>();
                    factory.TryAddDisposable(disposable);

                    using (InvokeMethodOnNewAssembly(SharedDictionaryManagerBase.GetCurrentDomainCachingFactory))
                    {
                        disposable.Received(1).Dispose();
                    }
                }
            }

            static IDisposable InvokeMethodOnNewAssembly<T>(Func<T> func)
            {
                var declaringType = func.Method.DeclaringType;
                var location = declaringType.Assembly.Location;
                var bytes = File.ReadAllBytes(location);
                var asm = Assembly.Load(bytes);
                var type = asm.GetType(declaringType.FullName);
                var method = type.GetMethod(func.Method.Name);
                return (IDisposable)method.Invoke(null, null);
            }
        }

        public class TheFixDesignTimeUriMethod
        {
            [TestCase("pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml", "pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
            [TestCase("file:///x:/solution/src/GitHub.VisualStudio.UI/SharedDictionary.xaml", "pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
            [TestCase("file:///x:/solution/src/GitHub.VisualStudio.UI/Styles/GitHubComboBox.xaml", "pack://application:,,,/GitHub.VisualStudio.UI;component/Styles/GitHubComboBox.xaml")]
            public void FixDesignTimeUri(string inUrl, string outUrl)
            {
                var inUri = ResourceDictionaryUtilities.ToPackUri(inUrl);

                var outUri = SharedDictionaryManagerBase.FixDesignTimeUri(inUri);

                Assert.That(outUri.ToString(), Is.EqualTo(outUrl));
            }
        }

        public class TheSourceProperty
        {
            [TestCase("pack://application:,,,/GitHub.UI;component/SharedDictionary.xaml")]
            public void IsEqualToSet(string url)
            {
                using (SharedDictionaryManagerBase.GetCurrentDomainCachingFactory())
                {
                    var uri = ResourceDictionaryUtilities.ToPackUri(url);
                    var target = new SharedDictionaryManagerBase();

                    target.Source = uri;

                    Assert.That(target.Source, Is.EqualTo(uri));
                }
            }
        }
    }
}
