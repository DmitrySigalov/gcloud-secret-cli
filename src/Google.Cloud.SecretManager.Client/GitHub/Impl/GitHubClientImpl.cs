using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.GitHub.Impl;

public class GitHubClientImpl : IGitHubClient
{
    private const string BASE_URI = "https://api.github.com/repos/DmitrySigalov/gclod-secret-manager-cli";
    
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public GitHubClientImpl(HttpClient httpClient, ILogger<GitHubClientImpl> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GitHubModel.Release> GetLatestReleaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogTrace("Getting latest release from GitHub API");
            
            var request = BuildHttpRequestMessage("releases", "latest");
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Request failed, status code: {response.StatusCode}.");
            }
            
            var result = await response.Content.ReadFromJsonAsync<GitHubModel.Release>(cancellationToken: cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while trying to get latest release from GitHub.");
        }

        return null;
    }

    private HttpRequestMessage BuildHttpRequestMessage(params string[] queryParameters)
    {
        var requestUrl = BuildRequestUrl("releases", "latest");
        
        return new HttpRequestMessage(HttpMethod.Get, requestUrl)
        {
            Headers = { { "Accept", "application/vnd.github.v3+json" }, {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36"} }
        };
    }
    
    private string BuildRequestUrl(params string[] queryParameters) =>
        $"{BASE_URI}/{string.Join('/', queryParameters)}";
}