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
            // First after help command is default
            .AddSingleton<ICommandHandler, SetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, GetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, GetSecretsWithProfileHandler>()
            .AddSingleton<ICommandHandler, ViewProfileHandler>()
            .AddSingleton<ICommandHandler, ConfigProfileCommandHandler>();

        return serviceCollection;
    }
}