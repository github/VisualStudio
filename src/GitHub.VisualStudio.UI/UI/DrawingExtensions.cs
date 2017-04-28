using System.Windows;

namespace GitHub.VisualStudio.UI
{
    public static class DrawingExtensions
    {
        public static T FreezeThis<T>(this T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }
    }
}
