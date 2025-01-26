using GCloud.Secret.Client.Google.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace GCloud.Secret.Client.Google;

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