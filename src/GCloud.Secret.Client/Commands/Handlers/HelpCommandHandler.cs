using ConsoleTables;
using GCloud.Secret.Client.Common;

namespace GCloud.Secret.Client.Commands.Handlers;

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

    public string ShortName => "?";

    public string Description => "";

    public Task<ContinueStatusEnum> Handle(CommandState commandState)
    {
        ConsoleHelper.WriteLineNotification($"Supported commands:");

        var table = new ConsoleTable("command-name", "short-name", "description");
        table.Options.EnableCount = false;

        foreach (var commandHandler in _commandHandlers)
        {
            table.AddRow(
                commandHandler.CommandName,
                commandHandler.ShortName,
                commandHandler.Description);
        }

        ConsoleHelper.Warn(() => table.Write(Format.Minimal));

        return Task.FromResult(ContinueStatusEnum.Exit);
    }
}