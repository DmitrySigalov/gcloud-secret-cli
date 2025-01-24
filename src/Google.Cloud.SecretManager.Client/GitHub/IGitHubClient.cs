namespace Google.Cloud.SecretManager.Client.GitHub;

public interface IGitHubClient
{
    Task<GitHubModel.Release> GetLatestReleaseAsync(CancellationToken cancellationToken);
}