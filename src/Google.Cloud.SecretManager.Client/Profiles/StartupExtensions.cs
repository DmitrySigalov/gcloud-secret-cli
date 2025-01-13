using Google.Cloud.SecretManager.Client.Profiles.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.Profiles;

public static class StartupExtensions
{
    public static IServiceCollection AddUserRuntimeServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IProfileConfigProvider, ProfileConfigProvider>();

        return serviceCollection;
    }
}