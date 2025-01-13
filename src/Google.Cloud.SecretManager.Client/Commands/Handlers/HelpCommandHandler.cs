using ConsoleTables;
using Google.Cloud.SecretManager.Client.Common;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class HelpCommandHandler : ICommandHandler
{
    private readonly ISet<ICommandHandler> _commandHandlers;

    public HelpCommandHandler(IEnumerable<ICommandHandler> commandHandlers)
    {
        _commandHandlers = new[] { this }
            .Union(commandHandlers)
            .ToHashSet();
    }

    public string CommandName => "help";

    public string Description => "";

    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"Supported commands:");

        var table = new ConsoleTable("command-name", "description");
        table.Options.EnableCount = false;

        foreach (var commandHandler in _commandHandlers)
        {
            table.AddRow(
                commandHandler.CommandName,
                commandHandler.Description);
        }

        ConsoleHelper.Warn(() => table.Write(Format.Minimal));

        return Task.CompletedTask;
    }
}