namespace Installer.Installers.Impl;

public class WindowsInstallerImpl : IInstaller
{
    public async Task RunAsync(InstallerArgs args)
    {
        var appPath = GetInstallPath(args);

        await args.RunDotnetPublishingAsync(appPath);

        AddAppToPathsAsync(appPath);
    }
    
    private static string GetInstallPath(InstallerArgs args)
    {
        var appPath = args.GetPathSetting("WinAppPath");
        
        Directory.CreateDirectory(appPath);

        return appPath;
    }
    
    private void AddAppToPathsAsync(string appPath)
    {
        Console.WriteLine("Adding app to machine path...");

        var name = "PATH";
        var scope = EnvironmentVariableTarget.Machine;
        var oldValue = Environment.GetEnvironmentVariable(name, scope)
            ?? string.Empty;

        if (oldValue.Contains(appPath + ";"))
        {
            Console.WriteLine($"No changed {name}: {oldValue}");
            Console.WriteLine();
            return;
        }

        var newPaths = GetNewWindowsPaths(oldValue, appPath);
        Environment.SetEnvironmentVariable(name, newPaths, scope);
        
        Console.WriteLine($"New {name}: {newPaths}");
        Console.WriteLine();
    }
    
    private static string GetNewWindowsPaths(string oldPath, string appPath)
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