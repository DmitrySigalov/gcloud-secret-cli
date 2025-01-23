namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class SetEnvCommandHandler : ICommandHandler
{
    public const string COMMAND_NAME = "set-env";

    public string CommandName => COMMAND_NAME;
    
    public string Description => "Set environment variables";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}