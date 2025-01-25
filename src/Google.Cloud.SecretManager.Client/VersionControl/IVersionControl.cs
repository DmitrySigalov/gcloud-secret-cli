namespace Google.Cloud.SecretManager.Client.VersionControl;

public interface IVersionControl
{
    Task CheckVersionAsync(CancellationToken cancellationToken);
}