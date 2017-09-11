using System;
using System.Windows.Controls;

namespace GitHub.UI.Helpers
{
    /// <summary>
    /// Extensions for TreeView control.
    /// </summary>
    public static class TreeViewExtensions
    {
        /// <summary>
        /// Gets the TreeViewItem for an item in the TreeView.
        /// </summary>
        /// <param name="tree">The tree view.</param>
        /// <param name="item">The item to search for.</param>
        /// <returns>The TreeViewItem or null if the item was null or not found.</returns>
        public static TreeViewItem GetTreeViewItem(this TreeView tree, object item)
        {
            if (item == null)
            {
                return null;
            }

            return GetTreeViewItem(tree.ItemContainerGenerator, item);
        }

        static TreeViewItem GetTreeViewItem(ItemContainerGenerator itemContainerGenerator, object item)
        {
            var container = (TreeViewItem)itemContainerGenerator.ContainerFromItem(item);

            if (container == null)
            {
                foreach (var childItem in itemContainerGenerator.Items)
                {
                    var node = (TreeViewItem)itemContainerGenerator.ContainerFromItem(childItem);
                    container = GetTreeViewItem(node.ItemContainerGenerator, item);

                    if (container != null)
                    {
                        return container;
                    }
                }
            }

            return container;
        }
    }
}
