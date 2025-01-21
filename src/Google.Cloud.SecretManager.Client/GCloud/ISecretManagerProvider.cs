namespace Google.Cloud.SecretManager.Client.GCloud;

public interface ISecretManagerProvider
{
    public Task<HashSet<string>> GetSecretIdsAsync(string projectId,
        CancellationToken cancellationToken = default);
}