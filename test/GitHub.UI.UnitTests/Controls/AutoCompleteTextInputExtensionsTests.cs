using GitHub.UI;
using GitHub.UI.Controls.AutoCompleteBox;
using Moq;
using Xunit;

class AutoCompleteTextInputExtensionsTests
{
    public class TheGetExpandedTextMethod
    {
        [Theory]
        [InlineData(":", 1, 0, ":apple: ")]
        [InlineData(":a", 2, 0, ":apple: ")]
        [InlineData(":ap", 3, 0, ":apple: ")]
        [InlineData(":a", 1, 0, ":apple: a")]
        [InlineData("Test :", 6, 5, "Test :apple: ")]
        [InlineData("Test :ap", 8, 5, "Test :apple: ")]
        [InlineData("Test :apother stuff", 8, 5, "Test :apple: other stuff")]
        public void ReturnsExpandedText(string text, int caretIndex, int completionOffset, string expected)
        {
            var textInput = Mock.Of<IAutoCompleteTextInput>(t => t.CaretIndex == caretIndex && t.Text == text);
            var expandedText = textInput.GetExpandedText(":apple:", completionOffset);
            Assert.Equal(expected, expandedText);
        }
    }
}
