namespace Google.Cloud.SecretManager.Client.GCloud;

public interface ISecretManagerProvider
{
    Task PocAsync(CancellationToken cancellationToken);
}