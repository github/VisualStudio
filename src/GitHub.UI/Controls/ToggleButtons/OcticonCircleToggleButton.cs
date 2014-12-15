using System.Windows;
using System.Windows.Controls.Primitives;

namespace GitHub.UI
{
    public class OcticonCircleToggleButton: OcticonToggleButton
    {
        static OcticonCircleToggleButton()
        {
            EventManager.RegisterClassHandler(
                typeof(OcticonCircleToggleButton), ClickEvent, new RoutedEventHandler(OnButtonClick));
        }

        static void OnButtonClick(object sender, RoutedEventArgs args)
        {
            ((OcticonCircleToggleButton)sender).OnToggle();
        }

        protected override void OnAccessKey(System.Windows.Input.AccessKeyEventArgs e)
        {
            base.OnAccessKey(e);

            OnToggle();
        }
    }
}