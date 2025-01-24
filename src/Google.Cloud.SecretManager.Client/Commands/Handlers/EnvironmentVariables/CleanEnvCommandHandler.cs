using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.EnvironmentVariables;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers.EnvironmentVariables;

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

        var currentEnvironmentDescriptor = _environmentVariablesProvider.Get() ?? new EnvironmentDescriptor();

        var newDescriptor = new EnvironmentDescriptor(); // Empty descriptor
        
        if (!string.IsNullOrEmpty(currentEnvironmentDescriptor?.ProfileName))
        {
            ConsoleHelper.WriteLineNotification(
                $"Start to deactivate profile [{currentEnvironmentDescriptor.ProfileName}] in the environment variables system");
        }

        _environmentVariablesProvider.Set(newDescriptor,
            ConsoleHelper.WriteLineNotification);

        Console.WriteLine();
        ConsoleHelper.WriteLineInfo(
            "DONE - Cleaned the environment variables system");

        return Task.CompletedTask;
    }
}