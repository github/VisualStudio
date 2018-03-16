using System;

namespace GitHub.ViewModels.Dialog
{
    [Flags]
    public enum LoginMode
    {
        None = 0x00,
        DotComOnly = 0x01,
        EnterpriseOnly = 0x02,
        DotComOrEnterprise = 0x03,
    }

    public interface ILoginCredentialsViewModel : IDialogContentViewModel
    {
        LoginMode LoginMode { get; }
        ILoginToGitHubViewModel GitHubLogin { get; }
        ILoginToGitHubForEnterpriseViewModel EnterpriseLogin { get; }
    }
}
