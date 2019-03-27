using System;
using System.Diagnostics;
using GitHub.Extensions;
using GitHub.Helpers;

namespace GitHub.UI.Controls.AutoCompleteBox
{
    public static class AutoCompleteTextInputExtensions
    {
        /// <summary>
        /// Given a text input and the current value, returns the expected new text.
        /// </summary>
        /// <param name="textInput"></param>
        /// <param name="value"></param>
        /// <param name="completionOffset"></param>
        /// <returns></returns>
        public static string GetExpandedText(this IAutoCompleteTextInput textInput, string value, int completionOffset)
        {
            Guard.ArgumentNotNull(textInput, "textInput");
            Guard.ArgumentNotNull(value, "value");

            int caretIndex = textInput.CaretIndex;
            int afterIndex = Math.Max(caretIndex, textInput.SelectionLength + textInput.SelectionStart);
            int offset = completionOffset;

            var currentText = textInput.Text ?? ""; // Playing it safe

            if (offset > currentText.Length) throw new InvalidOperationException("The offset can't be larger than the current text length");
            if (afterIndex > currentText.Length) throw new InvalidOperationException("The afterIndex can't be larger than the current text length");

            var before = currentText.Substring(0, offset);
            var after = currentText.Substring(afterIndex);
            string prefix = before + value + " ";
            return prefix + after;
        }
    }
}
