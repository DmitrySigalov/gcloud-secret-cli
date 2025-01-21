namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class GetEnvCommandHandler : ICommandHandler
{
    public string CommandName => "get-env";
    
    public string Description => "Get environment variables";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}