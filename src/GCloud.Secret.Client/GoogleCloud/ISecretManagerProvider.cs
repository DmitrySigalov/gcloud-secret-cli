using GCloud.Secret.Client.Profiles;

namespace GCloud.Secret.Client.GoogleCloud;

public interface ISecretManagerProvider
{
    Task<HashSet<string>> GetSecretIdsAsync(string projectId,
        CancellationToken cancellationToken = default);

    Task ApplySecretLatestValueAsync(string projectId,
        string secretId,
        SecretDetails secretDetails,
        CancellationToken cancellationToken = default);
}