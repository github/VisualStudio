using System;
using System.Windows.Media.Imaging;

namespace GitHub.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage CreateBitmapImage(string packUrl)
        {
            var bitmap = new BitmapImage(new Uri(packUrl));
            bitmap.Freeze();
            return bitmap;
        }
    }
}
