using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GitHub.UI
{
    public class GitHubComboBox : ComboBox
    {
        public override void OnApplyTemplate()
        {
            var popUp = GetTemplateChild("PART_Popup") as Popup;
            if (popUp != null)
            {
                popUp.CustomPopupPlacementCallback = PlacePopup;
            }
            base.OnApplyTemplate();
        }

        public static CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            return new[] { new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Vertical) };
        }
    }
}