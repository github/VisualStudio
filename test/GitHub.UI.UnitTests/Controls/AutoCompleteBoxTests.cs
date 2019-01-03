using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GitHub.Tests.TestHelpers;
using GitHub.UI;
using GitHub.UI.Helpers;
using Moq;
using Xunit;

public class AutoCompleteBoxTests
{
    public class TheItemsSourceProperty
    {
        [STAFact]
        public void SelectsFirstItemWhenSetToNonEmptyCollection()
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestions = new List<AutoCompleteSuggestion>
            {
                new AutoCompleteSuggestion("aaaa", obs, ":", ":"),
                new AutoCompleteSuggestion("bbbb", obs, ":", ":"),
                new AutoCompleteSuggestion("ccc", obs, ":", ":")
            };
            var result = new AutoCompleteResult(1, new ReadOnlyCollection<AutoCompleteSuggestion>(suggestions));
            var advisor = Mock.Of<IAutoCompleteAdvisor>(
                a => a.GetAutoCompletionSuggestions(Args.String, Args.Int32) == Observable.Return(result));
            var textBox = new TextBox();
            var autoCompleteBox = new AutoCompleteBox(Mock.Of<IDpiManager>())
            {
                SelectionAdapter = new SelectorSelectionAdapter(new ListBox()),
                Advisor = advisor,
                TextBox = new TextBoxAutoCompleteTextInput { TextBox = textBox }
            };

            textBox.Text = ":";

            Assert.Equal("aaaa", ((AutoCompleteSuggestion) autoCompleteBox.SelectedItem).Name);
            Assert.Equal(":", autoCompleteBox.Text); // It should not have expanded it yet
        }
    }

    public class TheIsDropDownOpenProperty
    {
        [STAFact]
        public void IsTrueWhenTextBoxChangesWithPrefixedValue()
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestions = new List<AutoCompleteSuggestion>
            {
                new AutoCompleteSuggestion("aaaa", obs, ":", ":"),
                new AutoCompleteSuggestion("bbbb", obs, ":", ":"),
                new AutoCompleteSuggestion("ccc", obs, ":", ":")
            };
            var result = new AutoCompleteResult(0, new ReadOnlyCollection<AutoCompleteSuggestion>(suggestions));
            var advisor = Mock.Of<IAutoCompleteAdvisor>(
                a => a.GetAutoCompletionSuggestions(Args.String, Args.Int32) == Observable.Return(result));
            var textBox = new TextBox();
            var autoCompleteBox = new AutoCompleteBox(Mock.Of<IDpiManager>())
            {
                SelectionAdapter = new SelectorSelectionAdapter(new ListBox()),
                Advisor = advisor,
                TextBox = new TextBoxAutoCompleteTextInput { TextBox = textBox }
            };

            textBox.Text = ":";

            Assert.True(autoCompleteBox.IsDropDownOpen);
        }

        [STAFact]
        public void IsFalseAfterASuggestionIsSelected()
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestions = new List<AutoCompleteSuggestion>
            {
                new AutoCompleteSuggestion("aaaa", obs, ":", ":"),
                new AutoCompleteSuggestion("bbbb", obs, ":", ":"),
                new AutoCompleteSuggestion("ccc", obs, ":", ":")
            };
            var result = new AutoCompleteResult(2, new ReadOnlyCollection<AutoCompleteSuggestion>(suggestions));
            var advisor = Mock.Of<IAutoCompleteAdvisor>(
                a => a.GetAutoCompletionSuggestions(Args.String, Args.Int32) == Observable.Return(result));
            var selectionAdapter = new TestSelectorSelectionAdapter();
            var textBox = new TextBox();
            var autoCompleteBox = new AutoCompleteBox(Mock.Of<IDpiManager>())
            {
                SelectionAdapter = selectionAdapter,
                Advisor = advisor,
                TextBox = new TextBoxAutoCompleteTextInput {TextBox = textBox}
            };
            textBox.Text = "A :a";
            textBox.CaretIndex = 4;
            Assert.Equal(4, textBox.CaretIndex);
            Assert.Equal(4, autoCompleteBox.TextBox.CaretIndex);
            Assert.True(autoCompleteBox.IsDropDownOpen);

            selectionAdapter.DoCommit();

            Assert.Equal("A :aaaa: ", textBox.Text);
            Assert.False(autoCompleteBox.IsDropDownOpen);
        }

        [STAFact]
        public void IsFalseAfterASuggestionIsCancelled()
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestions = new List<AutoCompleteSuggestion>
            {
                new AutoCompleteSuggestion("aaaa", obs, ":", ":"),
                new AutoCompleteSuggestion("bbbb", obs, ":", ":"),
                new AutoCompleteSuggestion("ccc", obs, ":", ":")
            };
            var result = new AutoCompleteResult(2, new ReadOnlyCollection<AutoCompleteSuggestion>(suggestions));
            var advisor = Mock.Of<IAutoCompleteAdvisor>(
                a => a.GetAutoCompletionSuggestions(Args.String, Args.Int32) == Observable.Return(result));
            var selectionAdapter = new TestSelectorSelectionAdapter();
            var textBox = new TextBox();
            var autoCompleteBox = new AutoCompleteBox(Mock.Of<IDpiManager>())
            {
                SelectionAdapter = selectionAdapter,
                Advisor = advisor,
                TextBox = new TextBoxAutoCompleteTextInput { TextBox = textBox }
            };
            textBox.Text = "A :a";
            textBox.CaretIndex = 4;
            Assert.Equal(4, textBox.CaretIndex);
            Assert.Equal(4, autoCompleteBox.TextBox.CaretIndex);
            Assert.True(autoCompleteBox.IsDropDownOpen);

            selectionAdapter.DoCancel();

            Assert.Equal("A :a", textBox.Text);
            Assert.False(autoCompleteBox.IsDropDownOpen);
        }

        [STAFact]
        public void HandlesKeyPressesToSelectAndCancelSelections()
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestions = new List<AutoCompleteSuggestion>
            {
                new AutoCompleteSuggestion("aaaa", obs, ":", ":"),
                new AutoCompleteSuggestion("bbbb", obs, ":", ":"),
                new AutoCompleteSuggestion("ccc", obs, ":", ":")
            };
            var result = new AutoCompleteResult(2, new ReadOnlyCollection<AutoCompleteSuggestion>(suggestions));
            var advisor = Mock.Of<IAutoCompleteAdvisor>(
                a => a.GetAutoCompletionSuggestions(Args.String, Args.Int32) == Observable.Return(result));
            var selectionAdapter = new TestSelectorSelectionAdapter();
            var textBox = new TextBox();
            var autoCompleteBox = new AutoCompleteBox(Mock.Of<IDpiManager>())
            {
                SelectionAdapter = selectionAdapter,
                Advisor = advisor,
                TextBox = new TextBoxAutoCompleteTextInput { TextBox = textBox }
            };
            textBox.Text = "A :a";
            textBox.CaretIndex = 4;
            Assert.Equal(4, textBox.CaretIndex);
            Assert.Equal(4, autoCompleteBox.TextBox.CaretIndex);
            Assert.True(autoCompleteBox.IsDropDownOpen);
            selectionAdapter.SelectorControl.SelectedIndex = 1; // Select the second item

            selectionAdapter.DoKeyDown(Key.Enter);

            Assert.Equal("A :bbbb: ", textBox.Text);
            Assert.False(autoCompleteBox.IsDropDownOpen);

            textBox.Text = "A :bbbb: :";
            textBox.CaretIndex = 10;

            // Ensure we can re-open the dropdown
            Assert.True(autoCompleteBox.IsDropDownOpen);

            selectionAdapter.DoKeyDown(Key.Escape);
            Assert.False(autoCompleteBox.IsDropDownOpen);
            Assert.Equal("A :bbbb: :", textBox.Text);
        }

        class TestSelectorSelectionAdapter : SelectorSelectionAdapter
        {
            public TestSelectorSelectionAdapter()
                : base(new ListBox())
            {
            }

            public void DoCommit()
            {
                base.OnCommit();
            }

            public void DoCancel()
            {
                base.OnCancel();
            }

            public void DoKeyDown(Key key)
            {
                var keyEventArgs = FakeKeyEventArgs.Create(key, false);
                HandleKeyDown(keyEventArgs);
            }
        }
    }
}
