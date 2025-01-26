using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.EnvironmentVariables;

namespace GCloud.Secret.Client.Commands.Handlers.EnvironmentVariables;

public class CleanEnvCommandHandler : ICommandHandler
{
    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

    public CleanEnvCommandHandler(
        IEnvironmentVariablesProvider environmentVariablesProvider)
    {
        _environmentVariablesProvider = environmentVariablesProvider;
    }
    
    public string CommandName => "clean-env";
    
    public string Description => "Clean environment variables";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var newDescriptor = new EnvironmentDescriptor(); // Empty descriptor
        
        _environmentVariablesProvider.Set(newDescriptor,
            ConsoleHelper.WriteLineNotification);

        Console.WriteLine();
        ConsoleHelper.WriteLineInfo(
            "DONE - Cleaned the environment variables system");

        return Task.CompletedTask;
    }
}