using GitHub.Helpers;
using NUnit.Framework;

namespace GitHub.UI.UnitTests
{
    public class SharedDictionaryManagerBaseTests
    {
        [TestCase("file:///x:/Project/src/GitHub.VisualStudio.UI/SharedDictionary.xaml", "pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
        [TestCase("file:///x:/Project/src/GitHub.VisualStudio.UI/Styles/Buttons.xaml", "file:///x:/Project/src/GitHub.VisualStudio.UI/Styles/Buttons.xaml")]
        [TestCase("pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml", "pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml")]
        public void FixDesignTimeUri(string inUrl, string outUrl)
        {
            var inUri = ResourceDictionaryUtilities.ToPackUri(inUrl);

            var outUri = SharedDictionaryManagerBase.FixDesignTimeUri(inUri);

            Assert.That(outUri.ToString(), Is.EqualTo(outUrl));
        }
    }
}
