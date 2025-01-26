namespace Google.Cloud.SecretManager.Client.Commands;

public interface ICommandHandler
{
    string CommandName { get; }

    string Description { get; }

    Task Handle(CancellationToken cancellationToken);
}