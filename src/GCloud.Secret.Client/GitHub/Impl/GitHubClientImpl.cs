using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.GitHub.Impl;

public class GitHubClientImpl : IGitHubClient
{
    private const string BASE_URI = "https://api.github.com/repos/DmitrySigalov/gclod-secret-manager-cli";
    
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public GitHubClientImpl(HttpClient httpClient,
        ILogger<GitHubClientImpl> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GitHubModel.Response<GitHubModel.Release>> GetLatestReleaseAsync(CancellationToken cancellationToken)
    {
        var result = new GitHubModel.Response<GitHubModel.Release>
        {
            RequestUrl = BuildRequestUrl("releases", "latest"),
        };
        
        try
        {
            var request = BuildHttpRequestMessage("releases", "latest");
            
            var response = await _httpClient.SendAsync(request, cancellationToken);

            result.StatusCode = response.StatusCode;
            result.IsSuccessStatusCode = response.IsSuccessStatusCode;
            
            if (result.IsSuccessStatusCode)
            {
                result.Data = await response.Content.ReadFromJsonAsync<GitHubModel.Release>(cancellationToken: cancellationToken);
            }
            else
            {
                _logger.LogError($"Get latest release from GitHub request failed, status code: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while trying to get latest release from GitHub");
        }

        return result;
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