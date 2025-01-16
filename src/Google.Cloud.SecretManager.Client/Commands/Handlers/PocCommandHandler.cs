using Google.Cloud.SecretManager.Client.GCloud;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class PocCommandHandler : ICommandHandler
{
    private readonly ISecretManagerProvider _profileConfigProvider;

    public PocCommandHandler(ISecretManagerProvider profileConfigProvider)
    {
        _profileConfigProvider = profileConfigProvider;
    }

    public string CommandName => "poc";

    public string Description => "Retrieve secrets from Google Cloud POC";
    
    public async Task Handle(CancellationToken cancellationToken)
    {
        await _profileConfigProvider.PocAsync(cancellationToken);
    }
}