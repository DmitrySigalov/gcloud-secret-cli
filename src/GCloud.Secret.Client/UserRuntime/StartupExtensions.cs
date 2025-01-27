using GCloud.Secret.Client.UserRuntime.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace GCloud.Secret.Client.UserRuntime;

public static class StartupExtensions
{
    public static IServiceCollection AddUserRuntimeServices(
        this IServiceCollection serviceCollection,
        string[] args)
    {
        var userParameters = new UserParameters
        {
            CommandName = args.FirstOrDefault(),
            Args = args,
        };

        serviceCollection
            .AddSingleton(userParameters)
            .AddSingleton<IUserFilesProvider, UserFilesProviderImpl>();

        return serviceCollection;
    }
}