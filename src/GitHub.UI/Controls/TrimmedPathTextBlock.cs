using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GitHub.UI
{
    /// <summary>
    /// TextBlock that displays a path and intelligently trims with ellipsis when the path doesn't
    /// fit in the allocated size.
    /// </summary>
    /// <remarks>
    /// When displaying a path that is too long for its allocated space, we need to trim the path
    /// with ellipses intelligently instead of simply trimming the end (as this is the filename
    /// which is the most important part!). This control trims a path in the following manner with
    /// decreasing allocated space:
    /// 
    /// - VisualStudio\src\GitHub.UI\Controls\TrimmedPathTextBlock.cs
    /// - VisualStudio\...\GitHub.UI\Controls\TrimmedPathTextBlock.cs
    /// - VisualStudio\...\...\Controls\TrimmedPathTextBlock.cs
    /// - VisualStudio\...\...\...\TrimmedPathTextBlock.cs
    /// - ...\...\...\...\TrimmedPathTextBlock.cs
    /// </remarks>
    public class TrimmedPathTextBlock : FrameworkElement
    {
        public static readonly DependencyProperty FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner(typeof(TrimmedPathTextBlock));
        public static readonly DependencyProperty FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner(typeof(TrimmedPathTextBlock));
        public static readonly DependencyProperty FontStretchProperty =
            TextBlock.FontStretchProperty.AddOwner(typeof(TrimmedPathTextBlock));
        public static readonly DependencyProperty FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner(typeof(TrimmedPathTextBlock));
        public static readonly DependencyProperty FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner(typeof(TrimmedPathTextBlock));
        public static readonly DependencyProperty ForegroundProperty =
            TextBlock.ForegroundProperty.AddOwner(typeof(TrimmedPathTextBlock));
        public static readonly DependencyProperty TextProperty =
            TextBlock.TextProperty.AddOwner(
                typeof(TrimmedPathTextBlock),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    TextChanged));

        FormattedText formattedText;
        FormattedText renderText;

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        protected FormattedText FormattedText
        {
            get
            {
                if (formattedText == null && Text != null)
                {
                    formattedText = CreateFormattedText(Text);
                }

                return formattedText;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Text == null)
            {
                return new Size();
            }

            var parts = Text
                .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar })
                .ToList();
            var nextPart = Math.Min(1, parts.Count - 1);

            while (true)
            {
                renderText = CreateFormattedText(string.Join(new string(Path.DirectorySeparatorChar, 1), parts));

                if (renderText.Width <= availableSize.Width || nextPart == -1)
                    break;

                parts[nextPart] = "\u2026";

                if (nextPart == 0)
                    nextPart = -1;
                else if (nextPart == parts.Count - 2)
                    nextPart = 0;
                else
                    nextPart++;
            };

            return new Size(renderText.Width, renderText.Height);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(renderText, new Point());
        }

        FormattedText CreateFormattedText(string text)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Foreground);
        }

        static void TextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TrimmedPathTextBlock;

            if (textBlock != null)
            {
                textBlock.formattedText = null;
                textBlock.renderText = null;
            }
        }
    }
}
