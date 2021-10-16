using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace GitHub.UI
{
    public class ClosePopupAction : TargetedTriggerAction<Popup>
    {
        protected override void Invoke(object parameter)
        {
            Target.IsOpen = false;
        }
    }
}