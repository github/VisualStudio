using GitHub.Helpers;
using NUnit.Framework;

namespace GitHub.UI.UnitTests
{
    public class SharedDictionaryManagerBaseTests
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
}
