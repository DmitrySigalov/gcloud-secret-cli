using GCloud.Secret.Client.Commands.Handlers;
using GCloud.Secret.Client.Commands.Handlers.ProfileConfiguration;
using GCloud.Secret.Client.Commands.Handlers.EnvironmentVariables;
using GCloud.Secret.Client.Commands.Handlers.Secrets;
using Microsoft.Extensions.DependencyInjection;

namespace GCloud.Secret.Client.Commands;

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
            .AddSingleton<ICommandHandler, ViewSecretsHandler>()
            .AddSingleton<ICommandHandler, SetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, CleanEnvCommandHandler>()
            .AddSingleton<ICommandHandler, CreateProfileCommandHandler>()
            .AddSingleton<ICommandHandler, ConfigProfileCommandHandler>()
            .AddSingleton<ICommandHandler, DeleteProfileCommandHandler>()
            .AddSingleton<ICommandHandler, ImportSecretsFromClipboardHandler>()
            .AddSingleton<ICommandHandler, ExportSecretsToClipboardHandler>();

        return serviceCollection;
    }
}