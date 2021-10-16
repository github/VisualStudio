namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// This service allows for functionality to be injected into the chain of different peek Comment ViewModel types.
    /// </summary>
    public interface ICommentService
    {
        /// <summary>
        /// This function uses MessageBox.Show to display a confirmation if a comment should be deleted.
        /// </summary>
        /// <returns></returns>
        bool ConfirmCommentDelete();
    }
}