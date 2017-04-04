using System;
using GitHub.Helpers;
using NUnit.Framework;

namespace GitHub.UI.UnitTests
{
    public class SharedDictionaryManagerBaseTests
    {
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
                var uri = ResourceDictionaryUtilities.ToPackUri(url);
                var target = new SharedDictionaryManagerBase();

                target.Source = uri;

                Assert.That(target.Source, Is.EqualTo(uri));
            }
        }
    }
}
