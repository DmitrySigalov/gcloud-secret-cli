using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.VersionControl;

public static class StartupExtensions
{
    public static IServiceCollection AddVersionControlServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IVersionControl, Impl.VersionControlImpl>();

        return serviceCollection;
    }
}