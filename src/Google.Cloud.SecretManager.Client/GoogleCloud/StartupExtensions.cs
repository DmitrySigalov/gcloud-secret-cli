using Google.Cloud.SecretManager.Client.GoogleCloud.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.GoogleCloud;

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