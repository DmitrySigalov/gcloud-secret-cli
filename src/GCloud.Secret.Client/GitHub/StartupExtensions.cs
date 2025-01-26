using GCloud.Secret.Client.GitHub.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace GCloud.Secret.Client.GitHub;

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