using Google.Cloud.SecretManager.Client.Commands.Handlers;
using Google.Cloud.SecretManager.Client.Commands.Handlers.EnvironmentVariables;
using Google.Cloud.SecretManager.Client.Commands.Handlers.SecretValues;
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
            .AddSingleton<ICommandHandler, ConfigProfileCommandHandler>()
            .AddSingleton<ICommandHandler, CleanEnvCommandHandler>()
            .AddSingleton<ICommandHandler, SetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, ViewProfileHandler>()
            .AddSingleton<ICommandHandler, DumpSecretsHandler>()
            .AddSingleton<ICommandHandler, ImportSecretsFromClipboardHandler>()
            .AddSingleton<ICommandHandler, ExportSecretsToClipboardHandler>();

        return serviceCollection;
    }
}