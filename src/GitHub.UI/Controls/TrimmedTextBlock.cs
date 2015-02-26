using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GitHub.UI
{
    public class TrimmedTextBlock : TextBlock
    {
        public static readonly DependencyProperty IsTextTrimmedProperty =
            DependencyProperty.Register("IsTextTrimmed", typeof(bool), typeof(TrimmedTextBlock), new UIPropertyMetadata(false));

        public bool IsTextTrimmed
        {
            get { return (bool)GetValue(IsTextTrimmedProperty); }
            set { SetValue(IsTextTrimmedProperty, value); }
        }

        /// <summary>
        /// Determines whether or not the text in the TextBlock is currently being
        /// trimmed due to width or height constraints.
        /// </summary>
        /// <remarks>Does not work properly when TextWrapping is set to WrapWithOverflow.</remarks>
        /// <returns><c>true</c> if the text is currently being trimmed; otherwise <c>false</c></returns>
        bool CalculateIsTextTrimmed()
        {
            if (!IsArrangeValid)
            {
                return IsTextTrimmed;
            }

            string text = Text;
            if (string.IsNullOrEmpty(Text) && Inlines.Count > 0)
            {
                text = string.Join("", Inlines.OfType<Run>().Select(x => x.Text));
            }

            var typeface = new Typeface(
                FontFamily,
                FontStyle,
                FontWeight,
                FontStretch);

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            var formattedText = new FormattedText(
                text,
                Thread.CurrentThread.CurrentCulture,
                FlowDirection,
                typeface,
                FontSize,
                Foreground);

            formattedText.MaxTextWidth = ActualWidth;

            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            return (Math.Floor(formattedText.Height) > Math.Floor(ActualHeight));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            SizeChanged += TrimmedTextBlockSizeChanged;
        }

        void TrimmedTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            IsTextTrimmed = TextTrimming.None != TextTrimming && CalculateIsTextTrimmed();

            foreach (var container in Inlines.OfType<InlineUIContainer>().ToArray())
            {
                var image = container.Child as EmojiImage;
                if (image == null) continue;

                var startF = container.ContentStart.GetCharacterRect(LogicalDirection.Forward);

                var isLast = Inlines.LastInline == container;
                image.UpdateVisibility(startF.Right, ActualWidth, isLast);
            }
        }
    }
}
