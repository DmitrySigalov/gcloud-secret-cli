using System.Runtime.InteropServices;
using Google.Cloud.SecretManager.Client.EnvironmentVariables.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables;

public static class StartupExtensions
{
    public static IServiceCollection AddEnvironmentVariablesServices(this IServiceCollection serviceCollection)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesProvider, WindowsEnvironmentVariablesProvider>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesProvider, OsxEnvironmentVariablesProvider>();
        }
        else
        {
            throw new NotSupportedException("OS not supported");
        }
        
        return serviceCollection;
    }
}