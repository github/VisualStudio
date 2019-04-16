using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.UI.UnitTests.Controls
{
    public class AutoCompleteBoxTests
    {
        [Apartment(ApartmentState.STA)]
        public class TheItemsSourceProperty
        {
            [Test]
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
                var advisor = Substitute.For<IAutoCompleteAdvisor>();
                advisor.GetAutoCompletionSuggestions(Arg.Any<string>(), Arg.Any<int>())
                    .Returns(Observable.Return(result));

                var textBox = new TextBox();
                var autoCompleteBox = new AutoCompleteBox(Substitute.For<IDpiManager>())
                {
                    SelectionAdapter = new SelectorSelectionAdapter(new ListBox()),
                    Advisor = advisor,
                    TextBox = new TextBoxAutoCompleteTextInput { TextBox = textBox }
                };

                textBox.Text = ":";

                Assert.That(((AutoCompleteSuggestion)autoCompleteBox.SelectedItem).Name, Is.EqualTo("aaaa"));
                Assert.That(autoCompleteBox.Text, Is.EqualTo(":"));
            }
        }

        [Apartment(ApartmentState.STA)]
        public class TheIsDropDownOpenProperty
        {
            [Test]
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
                var advisor = Substitute.For<IAutoCompleteAdvisor>();
                advisor.GetAutoCompletionSuggestions(Arg.Any<string>(), Arg.Any<int>())
                    .Returns(Observable.Return(result));

                var textBox = new TextBox();
                var autoCompleteBox = new AutoCompleteBox(Substitute.For<IDpiManager>())
                {
                    SelectionAdapter = new SelectorSelectionAdapter(new ListBox()),
                    Advisor = advisor,
                    TextBox = new TextBoxAutoCompleteTextInput { TextBox = textBox }
                };

                textBox.Text = ":";

                Assert.True(autoCompleteBox.IsDropDownOpen);
            }

            [Test]
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
                var advisor = Substitute.For<IAutoCompleteAdvisor>();
                advisor.GetAutoCompletionSuggestions(Arg.Any<string>(), Arg.Any<int>())
                    .Returns(Observable.Return(result));

                var selectionAdapter = new TestSelectorSelectionAdapter();
                var textBox = new TextBox();
                var autoCompleteBox = new AutoCompleteBox(Substitute.For<IDpiManager>())
                {
                    SelectionAdapter = selectionAdapter,
                    Advisor = advisor,
                    TextBox = new TextBoxAutoCompleteTextInput {TextBox = textBox}
                };
                textBox.Text = "A :a";
                textBox.CaretIndex = 4;
                Assert.AreEqual(4, textBox.CaretIndex);
                Assert.AreEqual(4, autoCompleteBox.TextBox.CaretIndex);
                Assert.True(autoCompleteBox.IsDropDownOpen);

                selectionAdapter.DoCommit();

                Assert.That(textBox.Text, Is.EqualTo("A :aaaa: "));
                Assert.False(autoCompleteBox.IsDropDownOpen);
            }

            [Test]
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
                var advisor = Substitute.For<IAutoCompleteAdvisor>();
                advisor.GetAutoCompletionSuggestions(Arg.Any<string>(), Arg.Any<int>())
                    .Returns(Observable.Return(result));

                var selectionAdapter = new TestSelectorSelectionAdapter();
                var textBox = new TextBox();
                var autoCompleteBox = new AutoCompleteBox(Substitute.For<IDpiManager>())
                {
                    SelectionAdapter = selectionAdapter,
                    Advisor = advisor,
                    TextBox = new TextBoxAutoCompleteTextInput { TextBox = textBox }
                };
                textBox.Text = "A :a";
                textBox.CaretIndex = 4;
                Assert.AreEqual(4, textBox.CaretIndex);
                Assert.AreEqual(4, autoCompleteBox.TextBox.CaretIndex);
                Assert.True(autoCompleteBox.IsDropDownOpen);

                selectionAdapter.DoCancel();

                Assert.That(textBox.Text, Is.EqualTo("A :a"));
                Assert.False(autoCompleteBox.IsDropDownOpen);
            }

            [Test]
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
                var advisor = Substitute.For<IAutoCompleteAdvisor>();
                advisor.GetAutoCompletionSuggestions(Arg.Any<string>(), Arg.Any<int>())
                    .Returns(Observable.Return(result));

                var selectionAdapter = new TestSelectorSelectionAdapter();
                var textBox = new TextBox();
                var autoCompleteBox = new AutoCompleteBox(Substitute.For<IDpiManager>())
                {
                    SelectionAdapter = selectionAdapter,
                    Advisor = advisor,
                    TextBox = new TextBoxAutoCompleteTextInput { TextBox = textBox }
                };
                textBox.Text = "A :a";
                textBox.CaretIndex = 4;
                Assert.AreEqual(4, textBox.CaretIndex);
                Assert.AreEqual(4, autoCompleteBox.TextBox.CaretIndex);
                Assert.True(autoCompleteBox.IsDropDownOpen);
                selectionAdapter.SelectorControl.SelectedIndex = 1; // Select the second item

                selectionAdapter.DoKeyDown(Key.Enter);

                Assert.AreEqual("A :bbbb: ", textBox.Text);
                Assert.False(autoCompleteBox.IsDropDownOpen);

                textBox.Text = "A :bbbb: :";
                textBox.CaretIndex = 10;

                // Ensure we can re-open the dropdown
                Assert.True(autoCompleteBox.IsDropDownOpen);

                selectionAdapter.DoKeyDown(Key.Escape);
                Assert.False(autoCompleteBox.IsDropDownOpen);
                Assert.AreEqual("A :bbbb: :", textBox.Text);
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

        public class FakeKeyEventArgs : KeyEventArgs
        {
            public static KeyEventArgs Create(Key realKey, bool isSystemKey, params Key[] pressedKeys)
            {
                return new FakeKeyEventArgs(realKey, isSystemKey, GetKeyStatesFromPressedKeys(pressedKeys));
            }

            public static KeyEventArgs Create(Key realKey, params Key[] pressedKeys)
            {
                return new FakeKeyEventArgs(realKey, false, GetKeyStatesFromPressedKeys(pressedKeys));
            }

            FakeKeyEventArgs(Key realKey, bool isSystemKey, IDictionary<Key, KeyStates> keyStatesMap) : base(GetKeyboardDevice(keyStatesMap), Substitute.For<PresentationSource>(), 1, realKey)
            {
                if (isSystemKey)
                {
                    MarkSystem();
                }
                RoutedEvent = ReflectionExtensions.CreateUninitialized<RoutedEvent>();
            }

            public void MarkSystem()
            {
                ReflectionExtensions.Invoke(this, "MarkSystem");
            }

            static KeyboardDevice GetKeyboardDevice(IDictionary<Key, KeyStates> keyStatesMap)
            {
                return new FakeKeyboardDevice(keyStatesMap);
            }

            static IDictionary<Key, KeyStates> GetKeyStatesFromPressedKeys(IEnumerable<Key> pressedKeys)
            {
                return pressedKeys == null ? null : pressedKeys.ToDictionary(k => k, k => KeyStates.Down);
            }
        }

        public class FakeKeyboardDevice : KeyboardDevice
        {
            readonly IDictionary<Key, KeyStates> keyStateMap;

            public FakeKeyboardDevice(IDictionary<Key, KeyStates> keyStateMap) : base(CreateFakeInputManager())
            {
                this.keyStateMap = keyStateMap ?? new Dictionary<Key, KeyStates>();
            }

            protected override KeyStates GetKeyStatesFromSystem(Key key)
            {
                KeyStates keyStates;
                keyStateMap.TryGetValue(key, out keyStates);
                return keyStates;
            }

            static InputManager CreateFakeInputManager()
            {
                Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(typeof(System.Security.Permissions.UIPermissionAttribute));
                // WARNING: This next call is pure evil, but ok here. See the note in the method implementation.
                return ReflectionExtensions.CreateUninitialized<InputManager>();
            }
        }

    }
}
