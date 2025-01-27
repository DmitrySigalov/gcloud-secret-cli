namespace GCloud.Secret.Client.Commands;

public interface ICommandHandler
{
    string CommandName { get; }

    string ShortName { get; }

    string Description { get; }

    Task<ContinueStatusEnum> Handle(CommandState commandState, CancellationToken cancellationToken);
}