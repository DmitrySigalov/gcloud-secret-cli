using GCloud.Secret.Client.Profiles.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace GCloud.Secret.Client.Profiles;

public static class StartupExtensions
{
    public static IServiceCollection AddProfileServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IProfileConfigProvider, ProfileConfigProviderImpl>();

        return serviceCollection;
    }
}