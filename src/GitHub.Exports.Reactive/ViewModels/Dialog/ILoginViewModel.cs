using System;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Represents the Login dialog content.
    /// </summary>
    public interface ILoginViewModel : IDialogContentViewModel
    {
        /// <summary>
        /// Gets the currently displayed login content.
        /// </summary>
        /// <remarks>
        /// The value of this property will either be a <see cref="ILoginCredentialsViewModel"/>
        /// or a <see cref="ILogin2FaViewModel"/>.
        /// </remarks>
        IDialogContentViewModel Content { get; }
    }
}
