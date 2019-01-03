using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GitHub.Helpers;
using GitHub.UI;
using Moq;
using Xunit;

public class AutoCompleteAdvisorTests
{
    public class TheParseAutoCompletionTokenMethod
    {
        [Theory]
        [InlineData(":", 1, "", 0)]
        [InlineData(":po", 3, "po", 0)]
        [InlineData(":po", 2, "p", 0)]
        [InlineData(":po or no :po", 2, "p", 0)]
        [InlineData(":po or no :po yo", 13, "po", 10)]
        [InlineData("This is :poo", 12, "poo", 8)]
        [InlineData("This is :poo or is it", 12, "poo", 8)]
        [InlineData("This is\r\n:poo or is it", 13, "poo", 9)]
        [InlineData("This is :poo or is it :zap:", 12, "poo", 8)]
        public void ParsesWordOffsetAndType(
            string text,
            int caretPosition,
            string expectedPrefix,
            int expectedOffset)
        {
            var token = AutoCompleteAdvisor.ParseAutoCompletionToken(text, caretPosition, ":");

            Assert.Equal(expectedPrefix, token.SearchSearchPrefix);
            Assert.Equal(expectedOffset, token.Offset);
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("foo bar", 0)]
        [InlineData("This has no special stuff", 5)]
        [InlineData("This has a : but caret is after the space after it", 13)]
        public void ReturnsNullForTextWithoutAnyTriggerCharactersMatchingCaretIndex(string text, int caretPosition)
        {
            Assert.Null(AutoCompleteAdvisor.ParseAutoCompletionToken(text, caretPosition, ":"));
        }

        [Theory]
        [InlineData("", 1)]
        [InlineData("", -1)]
        [InlineData("foo", 4)]
        [InlineData("foo", -1)]
        public void ThrowsExceptionWhenCaretIndexIsOutOfRangeOfText(string text, int caretIndex)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => AutoCompleteAdvisor.ParseAutoCompletionToken(text, caretIndex, ":"));
        }
    }

    public class TheGetAutoCompletionSuggestionsMethod
    {
        [Fact]
        public async Task ReturnsResultsWhenOnlyTokenTyped()
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestions = new List<AutoCompleteSuggestion>
            {
                new AutoCompleteSuggestion("rainbow", obs, ":", ":"),
                new AutoCompleteSuggestion("poop", obs, ":", ":"),
                new AutoCompleteSuggestion("poop_scoop", obs, ":", ":")
            }.ToObservable();
            var mentionsSource = Mock.Of<IAutoCompleteSource>(c =>
                c.GetSuggestions() == Observable.Empty<AutoCompleteSuggestion>() && c.Prefix == "@");
            var emojiSource = Mock.Of<IAutoCompleteSource>(c =>
                c.GetSuggestions() == suggestions && c.Prefix == ":");
            var advisor = new AutoCompleteAdvisor(new[] { mentionsSource, emojiSource });

            var result = await advisor.GetAutoCompletionSuggestions(":", 1);

            Assert.Equal(0, result.Offset);
            Assert.Equal(3, result.Suggestions.Count);
            Assert.Equal("poop", result.Suggestions[0].Name);
            Assert.Equal("poop_scoop", result.Suggestions[1].Name);
            Assert.Equal("rainbow", result.Suggestions[2].Name);
        }

        [Fact]
        public async Task ReturnsResultsWithNameMatchingToken()
        {
            var obs = Observable.Return(new BitmapImage());

            var suggestions = new List<AutoCompleteSuggestion>
            {
                new AutoCompleteSuggestion("rainbow", obs, ":", ":"),
                new AutoCompleteSuggestion("poop", obs, ":", ":"),
                new AutoCompleteSuggestion("poop_scoop", obs, ":", ":")
            }.ToObservable();
            var mentionsSource = Mock.Of<IAutoCompleteSource>(c =>
                c.GetSuggestions() == Observable.Empty<AutoCompleteSuggestion>() && c.Prefix == "@");
            var emojiSource = Mock.Of<IAutoCompleteSource>(c =>
                c.GetSuggestions() == suggestions && c.Prefix == ":");
            var advisor = new AutoCompleteAdvisor(new[] { mentionsSource, emojiSource });

            var result = await advisor.GetAutoCompletionSuggestions("this is :poo", 12);

            Assert.Equal(8, result.Offset);
            Assert.Equal(2, result.Suggestions.Count);
            Assert.Equal("poop", result.Suggestions[0].Name);
            Assert.Equal("poop_scoop", result.Suggestions[1].Name);
        }

        [Fact]
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
            var mentionsSource = Mock.Of<IAutoCompleteSource>(c =>
                c.GetSuggestions() == suggestions && c.Prefix == "@");
            var emojiSource = Mock.Of<IAutoCompleteSource>(c =>
                c.GetSuggestions() == Observable.Empty<AutoCompleteSuggestion>() && c.Prefix == ":");
            var advisor = new AutoCompleteAdvisor(new[] { mentionsSource, emojiSource });

            var result = await advisor.GetAutoCompletionSuggestions("this is @alice", 12);

            Assert.Equal(8, result.Offset);
            Assert.Equal(2, result.Suggestions.Count);
            Assert.Equal("loop", result.Suggestions[0].Name);
            Assert.Equal("poop", result.Suggestions[1].Name);
        }

        [Fact]
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
            var mentionsSource = Mock.Of<IAutoCompleteSource>(c =>
                c.GetSuggestions() == suggestions && c.Prefix == "@");
            var emojiSource = Mock.Of<IAutoCompleteSource>(c =>
                c.GetSuggestions() == Observable.Empty<AutoCompleteSuggestion>() && c.Prefix == ":");
            var advisor = new AutoCompleteAdvisor(new[] { mentionsSource, emojiSource });

            var result = await advisor.GetAutoCompletionSuggestions("this is @rainbow sucka", 16);

            Assert.Equal("rainbow", result.Suggestions[0].Name);
            Assert.Equal("rainbowbright", result.Suggestions[1].Name); 
            Assert.Equal("apricot", result.Suggestions[2].Name);
            Assert.Equal("bill", result.Suggestions[3].Name); // Bill and Zeke have the same name
            Assert.Equal("zeke", result.Suggestions[4].Name); // but the secondary sort is by login
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("Foo bar baz", 0)]
        [InlineData("Foo bar baz", 3)]
        public async Task ReturnsEmptyAutoCompleteResult(string text, int caretIndex)
        {
            var autoCompleteSource = Mock.Of<IAutoCompleteSource>(
                c => c.GetSuggestions() == Observable.Empty<AutoCompleteSuggestion>() && c.Prefix == ":");

            var advisor = new AutoCompleteAdvisor(new[] {autoCompleteSource});
            
            var result = await advisor.GetAutoCompletionSuggestions(text, 0);

            Assert.Same(AutoCompleteResult.Empty, result);
        }

        [Fact]
        public async Task ReturnsEmptyAutoCompleteResultWhenSourceThrowsException()
        {
            var autoCompleteSource = Mock.Of<IAutoCompleteSource>(
                c => c.GetSuggestions() == Observable.Throw<AutoCompleteSuggestion>(new Exception("FAIL!"))
                    && c.Prefix == "@");
            var advisor = new AutoCompleteAdvisor(new[] { autoCompleteSource });

            var result = await advisor.GetAutoCompletionSuggestions("@", 1);

            Assert.Same(AutoCompleteResult.Empty, result);
        }
    }
}
