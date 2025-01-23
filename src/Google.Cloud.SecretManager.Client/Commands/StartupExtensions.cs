using Google.Cloud.SecretManager.Client.Commands.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.Commands;

public static class StartupExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<CommandSelector>();

        serviceCollection
            .AddSingleton<HelpCommandHandler>()
            .AddSingleton<ConfigProfileCommandHandler>()
            // First after predefined command(s) is default
            .AddSingleton<ICommandHandler, SetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, ViewProfileHandler>()
            .AddSingleton<ICommandHandler, DumpSecretsHandler>()
            .AddSingleton<ICommandHandler, ImportSecretsFromClipboardHandler>()
            .AddSingleton<ICommandHandler, ExportSecretsToClipboardHandler>();

        return serviceCollection;
    }
}