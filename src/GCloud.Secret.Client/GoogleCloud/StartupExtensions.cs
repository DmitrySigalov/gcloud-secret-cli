using GCloud.Secret.Client.GoogleCloud.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace GCloud.Secret.Client.GoogleCloud;

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