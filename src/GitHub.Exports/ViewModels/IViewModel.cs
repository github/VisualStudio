using GitHub.UI;
using System.Windows.Input;

namespace GitHub.ViewModels
{
    public interface IViewModel
    {
        string Title { get; }
        ICommand Cancel { get; }
        bool IsShowing { get; }
        void Initialize(ViewWithData data);
        void Reset();
    }
}