using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// This service ensures the reuse of access tokens for the duration of their lifetime.
// It must be registered as a singleton service to work properly.
namespace Builder;

public class ApiTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UmbracoConfiguration _umbracoConfiguration;
    private readonly ILogger<ApiTokenService> _logger;

    private readonly Lock _lock = new();

    private string? _accessToken;
    private DateTime _accessTokenExpiry = DateTime.MinValue;

    public ApiTokenService(IHttpClientFactory httpClientFactory, IOptions<UmbracoConfiguration> umbracoConfiguration, ILogger<ApiTokenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _umbracoConfiguration = umbracoConfiguration.Value;
        _logger = logger;
    }

    public string? GetAccessToken()
    {
        if (_accessTokenExpiry > DateTime.UtcNow)
        {
            // we already have a token, reuse it.
            return _accessToken;
        }

        using (_lock.EnterScope())
        {
            if (_accessTokenExpiry > DateTime.UtcNow)
            {
                // another thread fetched a new token before this thread entered the lock, reuse it.
                return _accessToken;
            }

            var client = _httpClientFactory.CreateClient();
            var tokenResponse = client.RequestClientCredentialsTokenAsync(
                    new ClientCredentialsTokenRequest
                    {
                        Address = $"{_umbracoConfiguration.Host}/umbraco/management/api/v1/security/back-office/token",
                        ClientId = _umbracoConfiguration.ClientId,
                        ClientSecret =_umbracoConfiguration.ClientSecret 
                    }
                )
                // cannot await inside a using.
                .GetAwaiter().GetResult();

            if (tokenResponse.IsError || tokenResponse.AccessToken is null)
            {
                _logger.LogError("Could not obtain a token: {error}", tokenResponse.ErrorDescription);
                return null;
            }

            _accessToken = tokenResponse.AccessToken;
            _accessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 20);
            return _accessToken;
        }
    }
}