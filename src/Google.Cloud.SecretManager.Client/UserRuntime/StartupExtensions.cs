using Google.Cloud.SecretManager.Client.UserRuntime.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.UserRuntime;

public static class StartupExtensions
{
    public static IServiceCollection AddUserRuntimeServices(
        this IServiceCollection serviceCollection,
        string[] args)
    {
        var userParameters = new UserParameters
        {
            CommandName = args.FirstOrDefault(x => !x.StartsWith("-")),
            Args = args,
        };

        serviceCollection
            .AddSingleton(userParameters)
            .AddSingleton<IUserFilesProvider, UserFilesProviderImpl>();

        return serviceCollection;
    }
}