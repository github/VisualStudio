using GitHub.UI.Controls.AutoCompleteBox;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.UI.UnitTests.Controls
{
    class AutoCompleteTextInputExtensionsTests
    {
        public class TheGetExpandedTextMethod
        {
            [TestCase(":", 1, 0, ":apple: ")]
            [TestCase(":a", 2, 0, ":apple: ")]
            [TestCase(":ap", 3, 0, ":apple: ")]
            [TestCase(":a", 1, 0, ":apple: a")]
            [TestCase("Test :", 6, 5, "Test :apple: ")]
            [TestCase("Test :ap", 8, 5, "Test :apple: ")]
            [TestCase("Test :apother stuff", 8, 5, "Test :apple: other stuff")]
            public void ReturnsExpandedText(string text, int caretIndex, int completionOffset, string expected)
            {
                var textInput = Substitute.For<IAutoCompleteTextInput>();
                textInput.CaretIndex.Returns(caretIndex);
                textInput.Text.Returns(text);

                var expandedText = textInput.GetExpandedText(":apple:", completionOffset);
                Assert.AreEqual(expected, expandedText);
            }
        }
    }
}
