using Google.Cloud.SecretManager.Client.GCloud.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.GCloud;

public static class StartupExtensions
{
    public static IServiceCollection AddGoogleCloudServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<ISecretManagerProvider, SecretManagerProviderImpl>();

        return serviceCollection;
    }
}