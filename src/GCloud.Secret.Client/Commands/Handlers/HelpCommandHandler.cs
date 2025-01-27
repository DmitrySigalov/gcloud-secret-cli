using ConsoleTables;
using GCloud.Secret.Client.Common;

namespace GCloud.Secret.Client.Commands.Handlers;

public class HelpCommandHandler : ICommandHandler
{
    private const string CLI_NAME = "gscli"; 
    
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
        Console.WriteLine();

        PrintFormat();

        PrintSupportedCommands();

        return Task.FromResult(ContinueStatusEnum.Exit);
    }

    private void PrintFormat()
    {
        ConsoleHelper.WriteLineNotification($"{typeof(HelpCommandHandler).Assembly.GetName().Name} | {CLI_NAME} [[<command-name> | <short-name>] [<profile-name>]]");

        var table = new ConsoleTable("command-format", "description");
        table.Options.EnableCount = false;

        var descriptors = new Dictionary<string, string>
        {
            [$"{CLI_NAME}"] = "Interactive mode, select command and enter profile",
            [$"{CLI_NAME} <command>"] = "Interactive mode, enter profile",
            [$"{CLI_NAME} <command> <profile>"] = "Not interactive command execution",
        };

        foreach (var row in descriptors)
        {
            table.AddRow(
                row.Key,
                row.Value);
        }

        ConsoleHelper.Warn(() => table.Write(Format.Minimal));
    }

    private void PrintSupportedCommands()
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
    }
}