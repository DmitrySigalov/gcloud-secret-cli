using Google.Cloud.SecretManager.Client.Common;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class ConfigProfileCommandHandler : ICommandHandler
{
    public string CommandName => "config";
    
    public string Description => "Profile(s) configuration";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        return Task.CompletedTask;
    }
}