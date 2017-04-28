using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GitHub.UI;
using Xunit;

public class TwoFactorInputTests
{
    public class TheTextProperty : TestBaseClass
    {
#if !NCRUNCH
        [STAFact]
#endif
        public void SetsTextBoxesToIndividualCharacters()
        {
            var twoFactorInput = new TwoFactorInput();
            var textBoxes = GetChildrenRecursive(twoFactorInput).OfType<TextBox>().ToList();

            twoFactorInput.Text = "012345";

            Assert.Equal("012345", twoFactorInput.Text);
            Assert.Equal("0", textBoxes[0].Text);
            Assert.Equal("1", textBoxes[1].Text);
            Assert.Equal("2", textBoxes[2].Text);
            Assert.Equal("3", textBoxes[3].Text);
            Assert.Equal("4", textBoxes[4].Text);
            Assert.Equal("5", textBoxes[5].Text);
        }

#if !NCRUNCH
        [STAFact]
#endif
        public void IgnoresNonDigitCharacters()
        {
            var twoFactorInput = new TwoFactorInput();
            var textBoxes = GetChildrenRecursive(twoFactorInput).OfType<TextBox>().ToList();

            twoFactorInput.Text = "01xyz2345";

            Assert.Equal("012345", twoFactorInput.Text);
            Assert.Equal("0", textBoxes[0].Text);
            Assert.Equal("1", textBoxes[1].Text);
            Assert.Equal("2", textBoxes[2].Text);
            Assert.Equal("3", textBoxes[3].Text);
            Assert.Equal("4", textBoxes[4].Text);
            Assert.Equal("5", textBoxes[5].Text);
        }

#if !NCRUNCH
        [STAFact]
#endif
        public void HandlesNotEnoughCharacters()
        {
            var twoFactorInput = new TwoFactorInput();
            var textBoxes = GetChildrenRecursive(twoFactorInput).OfType<TextBox>().ToList();

            twoFactorInput.Text = "012";

            Assert.Equal("012", twoFactorInput.Text);
            Assert.Equal("0", textBoxes[0].Text);
            Assert.Equal("1", textBoxes[1].Text);
            Assert.Equal("2", textBoxes[2].Text);
            Assert.Equal("", textBoxes[3].Text);
            Assert.Equal("", textBoxes[4].Text);
            Assert.Equal("", textBoxes[5].Text);
        }

#if !NCRUNCH
        [STATheory]
#endif
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("xxxx", "")]
        public void HandlesNullAndStringsWithNoDigits(string input, string expected)
        {
            var twoFactorInput = new TwoFactorInput();
            var textBoxes = GetChildrenRecursive(twoFactorInput).OfType<TextBox>().ToList();

            twoFactorInput.Text = input;

            Assert.Equal(expected, twoFactorInput.Text);
            Assert.Equal("", textBoxes[0].Text);
            Assert.Equal("", textBoxes[1].Text);
            Assert.Equal("", textBoxes[2].Text);
            Assert.Equal("", textBoxes[3].Text);
            Assert.Equal("", textBoxes[4].Text);
            Assert.Equal("", textBoxes[5].Text);
        }

        static IEnumerable<FrameworkElement> GetChildrenRecursive(FrameworkElement element)
        {
            yield return element;
            foreach (var child in LogicalTreeHelper.GetChildren(element)
                .Cast<FrameworkElement>()
                .SelectMany(GetChildrenRecursive))
            {
                yield return child;
            }
        }
    }
}
