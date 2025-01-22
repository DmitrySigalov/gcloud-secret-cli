using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables;

public static class StartupExtensions
{
    public static IServiceCollection AddEnvironmentVariablesServices(this IServiceCollection serviceCollection)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // serviceCollection
            //     .AddSingleton<IEnvironmentVariablesProvider2, WindowsEnvironmentVariablesProvider2Impl>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // serviceCollection
            //     .AddSingleton<IEnvironmentVariablesProvider2, OsxEnvironmentVariablesProvider2Impl>();
        }
        else
        {
            throw new NotSupportedException("OS not supported");
        }
        
        return serviceCollection;
    }
}