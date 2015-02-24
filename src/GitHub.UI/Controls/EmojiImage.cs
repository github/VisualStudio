using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GitHub.UI
{
    /// <summary>
    /// This class is used to workaround some of the terrible issues that
    /// embedding an image inside a InlineCollection occurs.
    /// </summary>
    /// <remarks>
    /// - as changing the Visibility of the image triggers a reflow,
    ///    -> we trash the Source to achieve an equivalent effect
    /// - the position of the image can reduce as you adjust the textbox size
    ///    -> only store the largest value as that's the correct one
    /// </remarks>
    public class EmojiImage : Image
    {
        BitmapSource source;

        public void SetSource(BitmapSource bitmapImage)
        {
            source = bitmapImage;
            Show();
            Position = double.MinValue;
        }

        public void Show()
        {
            Source = source;
        }

        public void Hide()
        {
            Source = null;
        }

        public void UpdateVisibility(double offset, double actualWidth, bool isLast)
        {
            var end = offset + Width;

            if (!isLast)
            {
                // IF EMOJI IS NOT AT END, ADD 18 BECAUSE LOL FLOWDOCUMENTS
                end += 18;
            }

            Position = Math.Max(Position, end);

            if (Position >= actualWidth)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public double Position { get; private set; }
    }
}
