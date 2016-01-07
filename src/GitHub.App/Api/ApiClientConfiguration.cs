namespace GitHub.Api
{
    public partial class ApiClient : IApiClient
    {
        const string clientId = "";
        const string clientSecret = "";

        partial void Configure()
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
    }
}
