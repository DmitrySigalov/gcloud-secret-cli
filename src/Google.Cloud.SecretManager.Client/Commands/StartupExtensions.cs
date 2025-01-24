using Google.Cloud.SecretManager.Client.Commands.Handlers;
using Google.Cloud.SecretManager.Client.Commands.Handlers.EnvironmentVariables;
using Google.Cloud.SecretManager.Client.Commands.Handlers.Secrets;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.Commands;

public static class StartupExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<CommandSelector>();

        serviceCollection
            .AddSingleton<HelpCommandHandler>() // To prevent circular dependencies register as class
            // Regular commands
            .AddSingleton<ICommandHandler, GetSecretsHandler>()
            .AddSingleton<ICommandHandler, ViewAllHandler>()
            .AddSingleton<ICommandHandler, SetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, CleanEnvCommandHandler>()
            .AddSingleton<ICommandHandler, ConfigProfileCommandHandler>()
            .AddSingleton<ICommandHandler, ImportSecretsFromClipboardHandler>()
            .AddSingleton<ICommandHandler, ExportSecretsToClipboardHandler>();

        return serviceCollection;
    }
}