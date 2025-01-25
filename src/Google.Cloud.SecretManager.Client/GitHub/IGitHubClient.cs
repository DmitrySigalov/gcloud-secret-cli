namespace Google.Cloud.SecretManager.Client.GitHub;

public interface IGitHubClient
{
    Task<GitHubModel.Response<GitHubModel.Release>> GetLatestReleaseAsync(CancellationToken cancellationToken);
}