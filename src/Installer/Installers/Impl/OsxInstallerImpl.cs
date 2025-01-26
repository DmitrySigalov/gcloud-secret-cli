using System.Diagnostics;

namespace Installer.Installers.Impl;

public class OsxInstallerImpl : IInstaller
{
    public async Task RunAsync(InstallerArgs args)
    {
        var appPath = GetInstallPath(args);

        await args.RunDotnetPublishingAsync(appPath);

        await AddAppToPathsAsync(args, appPath);

        await MakeShortcutExecutableAsync(args, appPath);
    }
    
    private static string GetInstallPath(InstallerArgs args)
    {
        var appPath = args.GetPathSetting("OsxAppPath");
        
        Directory.CreateDirectory(appPath);

        return appPath;
    }
    
    private async Task AddAppToPathsAsync(InstallerArgs args, string appPath)
    {
        Console.WriteLine("Adding app to machine path...");

        var pathsFileName = args.GetPathSetting("OsxPathsD");
        
        await File.WriteAllTextAsync(pathsFileName, appPath, cancellationToken: args.CancellationToken);
        
        Console.WriteLine($"Created paths file: {pathsFileName}");
    }
    
    private static async Task MakeShortcutExecutableAsync(InstallerArgs args, string appPath)
    {
        var shortcut= args.GetSetting("OsxShortcut");
        
        Console.WriteLine($"Making '{shortcut}' execution...");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "chmod",
            WorkingDirectory = appPath,
            Arguments = $"+x {shortcut}"
        });

        await process!.WaitForExitAsync(args.CancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Making '{shortcut}' execution failed with exit code {process.ExitCode}");
        }
    }
}
