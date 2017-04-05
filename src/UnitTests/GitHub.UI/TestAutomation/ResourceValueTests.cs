using System.Resources;
using System.Collections;
using System.Globalization;
using Xunit;

namespace GitHub.UI.TestAutomation
{
    public class ResourceValueTest
    {
        [Fact]
        public void ValueAndNameAreTheSame()
        {
            ResourceSet autoIDResourceSet = AutomationIDs.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in autoIDResourceSet)
            {
                var key = entry.Key.ToString();
                var value = entry.Value.ToString();

                Assert.Equal(value, key);
            }
        }
    }
}