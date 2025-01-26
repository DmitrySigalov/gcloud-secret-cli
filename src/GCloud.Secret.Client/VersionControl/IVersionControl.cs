namespace GCloud.Secret.Client.VersionControl;

public interface IVersionControl
{
    Task CheckVersionAsync(CancellationToken cancellationToken);
}