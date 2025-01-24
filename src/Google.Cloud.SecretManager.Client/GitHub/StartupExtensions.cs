using Google.Cloud.SecretManager.Client.GitHub.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.GitHub;

public static class StartupExtensions
{
    public static IServiceCollection AddGitHubServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddHttpClient<IGitHubClient, GitHubClientImpl>();

        return serviceCollection;
    }
}