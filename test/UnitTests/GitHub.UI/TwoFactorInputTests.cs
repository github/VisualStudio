using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GitHub.UI;
using NUnit.Framework;

public class TwoFactorInputTests
{
    public class TheTextProperty : TestBaseClass
    {
        [Test]
        public void SetsTextBoxesToIndividualCharacters()
        {
            var twoFactorInput = new TwoFactorInput();
            var textBoxes = GetChildrenRecursive(twoFactorInput).OfType<TextBox>().ToList();

            twoFactorInput.Text = "012345";

            Assert.That("012345", Is.EqualTo(twoFactorInput.Text));
            Assert.That("0", Is.EqualTo(textBoxes[0].Text));
            Assert.That("1", Is.EqualTo(textBoxes[1].Text));
            Assert.That("2", Is.EqualTo(textBoxes[2].Text));
            Assert.That("3", Is.EqualTo(textBoxes[3].Text));
            Assert.That("4", Is.EqualTo(textBoxes[4].Text));
            Assert.That("5", Is.EqualTo(textBoxes[5].Text));
        }

        [Test]
        public void IgnoresNonDigitCharacters()
        {
            var twoFactorInput = new TwoFactorInput();
            var textBoxes = GetChildrenRecursive(twoFactorInput).OfType<TextBox>().ToList();

            twoFactorInput.Text = "01xyz2345";

            Assert.That("012345", Is.EqualTo(twoFactorInput.Text));
            Assert.That("0", Is.EqualTo(textBoxes[0].Text));
            Assert.That("1", Is.EqualTo(textBoxes[1].Text));
            Assert.That("2", Is.EqualTo(textBoxes[2].Text));
            Assert.That("3", Is.EqualTo(textBoxes[3].Text));
            Assert.That("4", Is.EqualTo(textBoxes[4].Text));
            Assert.That("5", Is.EqualTo(textBoxes[5].Text));
        }

        [Test]
        public void HandlesNotEnoughCharacters()
        {
            var twoFactorInput = new TwoFactorInput();
            var textBoxes = GetChildrenRecursive(twoFactorInput).OfType<TextBox>().ToList();

            twoFactorInput.Text = "012";

            Assert.That("012", Is.EqualTo(twoFactorInput.Text));
            Assert.That("0", Is.EqualTo(textBoxes[0].Text));
            Assert.That("1", Is.EqualTo(textBoxes[1].Text));
            Assert.That("2", Is.EqualTo(textBoxes[2].Text));
            Assert.That("", Is.EqualTo(textBoxes[3].Text));
            Assert.That("", Is.EqualTo(textBoxes[4].Text));
            Assert.That("", Is.EqualTo(textBoxes[5].Text));
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("xxxx", "")]
        public void HandlesNullAndStringsWithNoDigits(string input, string expected)
        {
            var twoFactorInput = new TwoFactorInput();
            var textBoxes = GetChildrenRecursive(twoFactorInput).OfType<TextBox>().ToList();

            twoFactorInput.Text = input;

            Assert.That(expected, Is.EqualTo(twoFactorInput.Text));
            Assert.That("", Is.EqualTo(textBoxes[0].Text));
            Assert.That("", Is.EqualTo(textBoxes[1].Text));
            Assert.That("", Is.EqualTo(textBoxes[2].Text));
            Assert.That("", Is.EqualTo(textBoxes[3].Text));
            Assert.That("", Is.EqualTo(textBoxes[4].Text));
            Assert.That("", Is.EqualTo(textBoxes[5].Text));
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
