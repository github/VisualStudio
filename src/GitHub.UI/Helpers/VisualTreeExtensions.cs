using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GitHub.Extensions;

namespace GitHub.UI.Helpers
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

        public static IEnumerable<Visual> GetSelfAndVisualDescendents(this Visual visual)
        {
            Guard.ArgumentNotNull(visual, nameof(visual));

            return Enumerable.Repeat(visual, 1).Concat(GetVisualDescendents(visual));
        }

        public static IEnumerable<Visual> GetVisualDescendents(this Visual visual)
        {
            Guard.ArgumentNotNull(visual, nameof(visual));

            var count = VisualTreeHelper.GetChildrenCount(visual);

            for (var i = 0; i < count; ++i)
            {
                var child = (Visual)VisualTreeHelper.GetChild(visual, i);
                yield return child;

                foreach (var descendent in child.GetVisualDescendents())
                {
                    yield return descendent;
                }
            }
        }
    }
}
