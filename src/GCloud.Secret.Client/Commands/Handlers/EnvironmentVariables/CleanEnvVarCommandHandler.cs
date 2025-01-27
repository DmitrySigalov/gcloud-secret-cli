using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.EnvironmentVariables;

namespace GCloud.Secret.Client.Commands.Handlers.EnvironmentVariables;

public class CleanEnvVarCommandHandler : ICommandHandler
{
    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

    public CleanEnvVarCommandHandler(
        IEnvironmentVariablesProvider environmentVariablesProvider)
    {
        _environmentVariablesProvider = environmentVariablesProvider;
    }
    
    public string CommandName => "clean-env-var";
    
    public string ShortName => "cev";

    public string Description => "Clean environment variables";
    
    public Task<ContinueStatusEnum> Handle(CommandState commandState)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var newDescriptor = new EnvironmentDescriptor(); // Empty descriptor
        
        _environmentVariablesProvider.Set(newDescriptor,
            ConsoleHelper.WriteLineNotification);

        Console.WriteLine();
        ConsoleHelper.WriteLineInfo(
            "DONE - Cleaned the environment variables system");

        return Task.FromResult(ContinueStatusEnum.Exit);
    }
}