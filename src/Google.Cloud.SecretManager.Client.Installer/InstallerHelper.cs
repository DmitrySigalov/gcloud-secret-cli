using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace Google.Cloud.SecretManager.Client.Installer;

public static class InstallerHelper
{
    public static string ClientAppName => "gcloud-secret-manager-cli";
    
    public static string ProjectPath => "src/Google.Cloud.SecretManager.Client/Google.Cloud.SecretManager.Client.csproj";
    
    public static string GetBuildWorkingDirectory(IConfiguration configuration)
    {
        var path = configuration["buildWorkingDirectory"];
        if (!string.IsNullOrEmpty(path))
        {
            return path;
        }
        
        return "../";
    }
    
    public static string GetAppHomeDirectory(IConfiguration configuration)
    {
        var path = configuration["appHomePath"];
        if (!string.IsNullOrEmpty(path))
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            path = Path.Combine(
                GetBuildWorkingDirectory(configuration),
                path);

            path = Path.GetFullPath(path);

            return path;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            return Path.Combine(windowsPath, ClientAppName);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"/usr/local/share/{ClientAppName}";
        }

        throw new NotSupportedException($"Not supported {RuntimeInformation.RuntimeIdentifier}");
    }
    
    public static bool ShouldUpdateWindowsPaths(string oldPath, string appPath) =>
        !oldPath.Contains(appPath, StringComparison.InvariantCultureIgnoreCase);
    
    public static string GetNewWindowsPaths(string oldPath, string appPath)
    {
        string newValue;

        if (oldPath.EndsWith(';'))
        {
            newValue = oldPath + $"{appPath};";
        }
        else
        {
            newValue = oldPath + $";{appPath};";
        }

        return newValue;
    }
}