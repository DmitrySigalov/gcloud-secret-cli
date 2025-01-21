using Google.Cloud.SecretManager.Client.Profiles;

namespace Google.Cloud.SecretManager.Client.GCloud;

public interface ISecretManagerProvider
{
    Task<HashSet<string>> GetSecretIdsAsync(string projectId,
        CancellationToken cancellationToken = default);

    Task ApplySecretLatestValueAsync(string projectId,
        string secretId,
        SecretDetails secretDetails,
        CancellationToken cancellationToken = default);
}