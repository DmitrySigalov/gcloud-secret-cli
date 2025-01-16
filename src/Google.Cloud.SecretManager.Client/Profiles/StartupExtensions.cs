using Google.Cloud.SecretManager.Client.Profiles.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.Profiles;

public static class StartupExtensions
{
    public static IServiceCollection AddProfileServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IProfileConfigProvider, ProfileConfigProvider>();

        return serviceCollection;
    }
}