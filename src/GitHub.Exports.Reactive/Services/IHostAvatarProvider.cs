namespace GitHub.Services
{
    public interface IHostAvatarProvider
    {
        /// <summary>
        /// Gets an avatar provider for the given GitHub base URL (ie
        /// either github.com or an enterprise host).
        /// </summary>
        IAvatarProvider Get(string gitHubBaseUrl);
    }
}
