using System.Diagnostics;

namespace Installer.Installers;

public static class InstallerArgsExtensions
{
    public static string GetPathSetting(this InstallerArgs args,
        string argumentName)
    {
        var result = args.GetSetting(argumentName);
        
        return Path.GetFullPath(result);
    }

    public static string GetSetting(this InstallerArgs args,
        string argumentName)
    {
        if (!ApplicationSettings.DefaultArguments.TryGetValue(argumentName, out var resultGetter))
        {
            throw new NotSupportedException($"Unknown configuration argument: {argumentName}");
        }

        var result = args.Configuration[argumentName];

        if (string.IsNullOrEmpty(result))
        {
            result = resultGetter();
        }

        return result;
    }

    public static async Task RunDotnetPublishingAsync(this InstallerArgs args, string appPath)
    {
        var buildWorkingDirectory = args.GetPathSetting("BuildWorkingDirectory");
        var projectPath = ApplicationSettings.ProjectPath;
        
        Console.WriteLine("Publishing...");
        
        Console.WriteLine($"Build working directory: {buildWorkingDirectory}");
        Console.WriteLine($"Build project path: {projectPath}");
        Console.WriteLine($"Application path: {appPath}");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = buildWorkingDirectory,
            Arguments =
                $"publish {projectPath} --output {appPath} --source https://api.nuget.org/v3/index.json --configuration Release --verbosity quiet /property:WarningLevel=0"
        });

        await process!.WaitForExitAsync(args.CancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Publishing failed with exit code {process.ExitCode}");
        }
        
        Console.WriteLine();
    }
}