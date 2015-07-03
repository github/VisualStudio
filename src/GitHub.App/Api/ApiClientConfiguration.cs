namespace GitHub.Api
{
    public partial class ApiClient : IApiClient
    {
        const string clientId = "YOUR CLIENT ID HERE";
        const string clientSecret = "YOUR CLIENT SECRET HERE";

        partial void Configure()
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
    }
}
