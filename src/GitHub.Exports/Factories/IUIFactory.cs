using GitHub.Exports;

namespace GitHub.App.Factories
{
    public interface IUIFactory
    {
        IUIPair CreateViewAndViewModel(UIViewType viewType);
    }
}