using System.Resources;
using System.Collections;
using System.Globalization;
using NUnit.Framework;

namespace GitHub.UI.TestAutomation
{
    public class ResourceValueTest
    {
        [Test]
        public void ValueAndNameAreTheSame()
        {
            ResourceSet autoIDResourceSet = AutomationIDs.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in autoIDResourceSet)
            {
                var key = entry.Key.ToString();
                var value = entry.Value.ToString();

                Assert.That(value, Is.EqualTo(key));
            }
        }
    }
}