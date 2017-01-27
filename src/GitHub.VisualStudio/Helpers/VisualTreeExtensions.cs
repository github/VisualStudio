using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Helpers
{
    public static class VisualTreeExtensions
    {
        public static IEnumerable<Visual> GetSelfAndVisualAncestors(this Visual visual)
        {
            Guard.ArgumentNotNull(visual, nameof(visual));

            return Enumerable.Repeat(visual, 1).Concat(GetVisualAncestors(visual));
        }

        public static IEnumerable<Visual> GetVisualAncestors(this Visual visual)
        {
            Guard.ArgumentNotNull(visual, nameof(visual));

            while (true)
            {
                visual = VisualTreeHelper.GetParent(visual) as Visual;

                if (visual != null)
                    yield return visual;
                else
                    break;
            }
        }
    }
}
