using Google.Cloud.SecretManager.Client.GCloud;
using Google.Cloud.SecretManager.Client.GCloud.Impl;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class PocCommandHandler : ICommandHandler
{
    public string CommandName => "poc";

    public string Description => "Retrieve secrets from Google Cloud POC";
    
    public async Task Handle(CancellationToken cancellationToken)
    {
        await PocTests.RunAsync(cancellationToken);
    }
}