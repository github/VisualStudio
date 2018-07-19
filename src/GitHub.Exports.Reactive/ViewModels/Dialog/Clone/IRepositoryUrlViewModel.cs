using System;

namespace GitHub.ViewModels.Dialog.Clone
{
    public interface IRepositoryUrlViewModel : IRepositoryCloneTabViewModel
    {
        string Url { get; set; }
    }
}
