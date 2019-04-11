using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.UI.UnitTests.Helpers
{
    public class AutoCompleteAdvisorTests
    {
        public class TheParseAutoCompletionTokenMethod
        {
            [TestCase(":", 1, "", 0)]
            [TestCase(":po", 3, "po", 0)]
            [TestCase(":po", 2, "p", 0)]
            [TestCase(":po or no :po", 2, "p", 0)]
            [TestCase(":po or no :po yo", 13, "po", 10)]
            [TestCase("This is :poo", 12, "poo", 8)]
            [TestCase("This is :poo or is it", 12, "poo", 8)]
            [TestCase("This is\r\n:poo or is it", 13, "poo", 9)]
            [TestCase("This is :poo or is it :zap:", 12, "poo", 8)]
            public void ParsesWordOffsetAndType(
                string text,
                int caretPosition,
                string expectedPrefix,
                int expectedOffset)
            {
                var token = AutoCompleteAdvisor.ParseAutoCompletionToken(text, caretPosition, ":");

                Assert.AreEqual(expectedPrefix, token.SearchSearchPrefix);
                Assert.AreEqual(expectedOffset, token.Offset);
            }

            [TestCase("", 0)]
            [TestCase("foo bar", 0)]
            [TestCase("This has no special stuff", 5)]
            [TestCase("This has a : but caret is after the space after it", 13)]
            public void ReturnsNullForTextWithoutAnyTriggerCharactersMatchingCaretIndex(string text, int caretPosition)
            {
                Assert.Null(AutoCompleteAdvisor.ParseAutoCompletionToken(text, caretPosition, ":"));
            }

            [TestCase("", 1)]
            [TestCase("", -1)]
            [TestCase("foo", 4)]
            [TestCase("foo", -1)]
            public void ThrowsExceptionWhenCaretIndexIsOutOfRangeOfText(string text, int caretIndex)
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => AutoCompleteAdvisor.ParseAutoCompletionToken(text, caretIndex, ":"));
            }
        }

        public class TheGetAutoCompletionSuggestionsMethod
        {
            [Test]
            public async Task ReturnsResultsWhenOnlyTokenTyped()
            {
                var obs = Observable.Return(new BitmapImage());

                var suggestions = new List<AutoCompleteSuggestion>
                {
                    new AutoCompleteSuggestion("rainbow", obs, ":", ":"),
                    new AutoCompleteSuggestion("poop", obs, ":", ":"),
                    new AutoCompleteSuggestion("poop_scoop", obs, ":", ":")
                }.ToObservable();

                var mentionsSource = Substitute.For<IAutoCompleteSource>();
                mentionsSource.GetSuggestions().Returns(Observable.Empty<AutoCompleteSuggestion>());
                mentionsSource.Prefix.Returns("@");

                var emojiSource = Substitute.For<IAutoCompleteSource>();
                emojiSource.GetSuggestions().Returns(suggestions);
                emojiSource.Prefix.Returns(":");

                var advisor = new AutoCompleteAdvisor(new[] { mentionsSource, emojiSource });

                var result = await advisor.GetAutoCompletionSuggestions(":", 1);

                Assert.AreEqual(0, result.Offset);
                Assert.AreEqual(3, result.Suggestions.Count);
                Assert.AreEqual("poop", result.Suggestions[0].Name);
                Assert.AreEqual("poop_scoop", result.Suggestions[1].Name);
                Assert.AreEqual("rainbow", result.Suggestions[2].Name);
            }

            [Test]
            public async Task ReturnsResultsWithNameMatchingToken()
            {
                var obs = Observable.Return(new BitmapImage());

                var suggestions = new List<AutoCompleteSuggestion>
                {
                    new AutoCompleteSuggestion("rainbow", obs, ":", ":"),
                    new AutoCompleteSuggestion("poop", obs, ":", ":"),
                    new AutoCompleteSuggestion("poop_scoop", obs, ":", ":")
                }.ToObservable();

                var mentionsSource = Substitute.For<IAutoCompleteSource>();
                mentionsSource.GetSuggestions().Returns(Observable.Empty<AutoCompleteSuggestion>());
                mentionsSource.Prefix.Returns("@");

                var emojiSource = Substitute.For<IAutoCompleteSource>();
                emojiSource.GetSuggestions().Returns(suggestions);
                emojiSource.Prefix.Returns(":");

                var advisor = new AutoCompleteAdvisor(new[] { mentionsSource, emojiSource });

                var result = await advisor.GetAutoCompletionSuggestions("this is :poo", 12);

                Assert.AreEqual(8, result.Offset);
                Assert.AreEqual(2, result.Suggestions.Count);
                Assert.AreEqual("poop", result.Suggestions[0].Name);
                Assert.AreEqual("poop_scoop", result.Suggestions[1].Name);
            }

            [Test]
            public async Task ReturnsResultsWithDescriptionMatchingToken()
            {
                var obs = Observable.Return(new BitmapImage());

                var suggestions = new List<AutoCompleteSuggestion>
                {
                    new AutoCompleteSuggestion("rainbow", "John Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("poop", "Alice Bob", obs, "@", ""),
                    new AutoCompleteSuggestion("poop_scoop", obs, "@", ""),
                    new AutoCompleteSuggestion("loop", "Jimmy Alice Cooper", obs, "@", ""),
                }.ToObservable();

                var mentionsSource = Substitute.For<IAutoCompleteSource>();
                mentionsSource.GetSuggestions().Returns(suggestions);
                mentionsSource.Prefix.Returns("@");

                var emojiSource = Substitute.For<IAutoCompleteSource>();
                emojiSource.GetSuggestions().Returns(Observable.Empty<AutoCompleteSuggestion>());
                emojiSource.Prefix.Returns(":");

                var advisor = new AutoCompleteAdvisor(new[] { mentionsSource, emojiSource });

                var result = await advisor.GetAutoCompletionSuggestions("this is @alice", 12);

                Assert.AreEqual(8, result.Offset);
                Assert.AreEqual(2, result.Suggestions.Count);
                Assert.AreEqual("loop", result.Suggestions[0].Name);
                Assert.AreEqual("poop", result.Suggestions[1].Name);
            }

            [Test]
            public async Task ReturnsMentionsInCorrectOrder()
            {
                var obs = Observable.Return(new BitmapImage());

                var suggestions = new List<AutoCompleteSuggestion>
                {
                    // We need to have more than 10 matches to ensure we grab the most appropriate top ten
                    new AutoCompleteSuggestion("zztop1", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop2", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop3", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop4", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop5", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop6", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop7", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop8", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop9", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("zztop10", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("rainbowbright", "Jimmy Alice Cooper", obs, "@", ""),
                    new AutoCompleteSuggestion("apricot", "Bob Rainbow", obs, "@", ""),
                    new AutoCompleteSuggestion("rainbow", "John Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("poop_scoop", obs, "@", ""),
                    new AutoCompleteSuggestion("zeke", "RainbowBright Doe", obs, "@", ""),
                    new AutoCompleteSuggestion("bill", "RainbowBright Doe", obs, "@", "")
                }.ToObservable();

                var mentionsSource = Substitute.For<IAutoCompleteSource>();
                mentionsSource.GetSuggestions().Returns(suggestions);
                mentionsSource.Prefix.Returns("@");

                var emojiSource = Substitute.For<IAutoCompleteSource>();
                emojiSource.GetSuggestions().Returns(Observable.Empty<AutoCompleteSuggestion>());
                emojiSource.Prefix.Returns(":");

                var advisor = new AutoCompleteAdvisor(new[] { mentionsSource, emojiSource });

                var result = await advisor.GetAutoCompletionSuggestions("this is @rainbow sucka", 16);

                Assert.AreEqual("rainbow", result.Suggestions[0].Name);
                Assert.AreEqual("rainbowbright", result.Suggestions[1].Name); 
                Assert.AreEqual("apricot", result.Suggestions[2].Name);
                Assert.AreEqual("bill", result.Suggestions[3].Name); // Bill and Zeke have the same name
                Assert.AreEqual("zeke", result.Suggestions[4].Name); // but the secondary sort is by login
            }

            [Theory]
            [TestCase("", 0)]
            [TestCase("Foo bar baz", 0)]
            [TestCase("Foo bar baz", 3)]
            public async Task ReturnsEmptyAutoCompleteResult(string text, int caretIndex)
            {
                var autoCompleteSource = Substitute.For<IAutoCompleteSource>();
                autoCompleteSource.GetSuggestions().Returns(Observable.Empty<AutoCompleteSuggestion>());
                autoCompleteSource.Prefix.Returns(":");

                var advisor = new AutoCompleteAdvisor(new[] {autoCompleteSource});
            
                var result = await advisor.GetAutoCompletionSuggestions(text, 0);

                Assert.AreSame(AutoCompleteResult.Empty, result);
            }

            [Test]
            public async Task ReturnsEmptyAutoCompleteResultWhenSourceThrowsException()
            {
                var autoCompleteSource = Substitute.For<IAutoCompleteSource>();
                autoCompleteSource.GetSuggestions().Returns(Observable.Throw<AutoCompleteSuggestion>(new Exception("FAIL!")));
                autoCompleteSource.Prefix.Returns("@");

                var advisor = new AutoCompleteAdvisor(new[] { autoCompleteSource });

                var result = await advisor.GetAutoCompletionSuggestions("@", 1);

                Assert.AreSame(AutoCompleteResult.Empty, result);
            }
        }
    }
}
