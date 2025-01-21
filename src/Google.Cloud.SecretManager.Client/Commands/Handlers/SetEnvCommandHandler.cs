namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class SetEnvCommandHandler : ICommandHandler
{
    public string CommandName => "set-env";
    
    public string Description => "Set environment variables";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}