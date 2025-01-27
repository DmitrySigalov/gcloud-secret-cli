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
        PrintUsage();

        PrintSupportedCommands();

        return Task.FromResult(ContinueStatusEnum.Exit);
    }

    private void PrintUsage()
    {
        ConsoleHelper.WriteLineInfo("Usage:");
        ConsoleHelper.WriteLineWarn($"{typeof(HelpCommandHandler).Assembly.GetName().Name} | {CLI_NAME} [[<command-name> | <short-name>] [<profile-name>]]");
        
        var table = new ConsoleTable("command", "description");
        table.Options.EnableCount = false;

        var descriptors = new Dictionary<string, string>
        {
            [$"{CLI_NAME}"] = "Interactive mode, select command and profile",
            [$"{CLI_NAME} <command>"] = "Interactive mode, select profile",
            [$"{CLI_NAME} <command> <profile>"] = "Not-interactive command execution",
        };

        foreach (var row in descriptors)
        {
            table.AddRow(
                row.Key,
                row.Value);
        }

        table.Write(Format.Minimal);
    }

    private void PrintSupportedCommands()
    {
        ConsoleHelper.WriteLineInfo("Supported commands:");

        var table = new ConsoleTable("command-name", "short-name", "description");
        table.Options.EnableCount = false;

        foreach (var commandHandler in _commandHandlers)
        {
            table.AddRow(
                commandHandler.CommandName,
                commandHandler.ShortName,
                commandHandler.Description);
        }

        table.Write(Format.Minimal);
    }
}