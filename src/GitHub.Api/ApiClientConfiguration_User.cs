using System;

namespace GitHub.Api
{
    static partial class ApiClientConfiguration
    {
        const string clientId = "YOUR CLIENT ID HERE";
        const string clientSecret = "YOUR CLIENT SECRET HERE";

        static partial void Configure()
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
    }
}
