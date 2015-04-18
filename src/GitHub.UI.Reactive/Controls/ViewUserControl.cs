using GitHub.UI.Helpers;

namespace GitHub.UI
{
    /// <summary>
    /// Base class for all of our user controls This one imports GitHub resources/styles.
    /// </summary>

    public class ViewUserControl : SimpleViewUserControl
    {
        public ViewUserControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
        }
    }
}
