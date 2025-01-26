namespace GCloud.Secret.Client.Commands;

public interface ICommandHandler
{
    string CommandName { get; }

    string Description { get; }

    Task<ResultStatusEnum> Handle(CommandState state, CancellationToken cancellationToken);
}