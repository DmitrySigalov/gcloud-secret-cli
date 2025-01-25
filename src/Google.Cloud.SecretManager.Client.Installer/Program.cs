// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.InteropServices;
using Google.Cloud.SecretManager.Client.Installer;
using Microsoft.Extensions.Configuration;

Console.WriteLine($"Installing {InstallerHelper.ClientAppName}...");

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
    !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    throw new NotSupportedException($"Not supported {RuntimeInformation.RuntimeIdentifier}");
}


var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

var buildWorkingDirectory = InstallerHelper.GetBuildWorkingDirectory(configuration);
var appHomePath = InstallerHelper.GetAppHomeDirectory(configuration);

Console.WriteLine("Installing...");

var process = Process.Start(new ProcessStartInfo
{
    FileName = "dotnet",
    WorkingDirectory = buildWorkingDirectory,
    Arguments =
        $"publish {InstallerHelper.ProjectPath} --output {appHomePath} --source https://api.nuget.org/v3/index.json --configuration Release --verbosity quiet /property:WarningLevel=0"
});

await process!.WaitForExitAsync();

Console.WriteLine("Adding app to machine path...");

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var name = "PATH";
    var scope = EnvironmentVariableTarget.Machine;
    var oldValue = Environment.GetEnvironmentVariable(name, scope);

    if (InstallerHelper.ShouldUpdateWindowsPaths(oldValue!, appHomePath))
    {
        var newPaths = InstallerHelper.GetNewWindowsPaths(oldValue!, appHomePath);
        Environment.SetEnvironmentVariable(name, newPaths, scope);
    }
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    var pathsFile = $"/etc/paths.d/{InstallerHelper.ClientAppName}";

    await File.WriteAllTextAsync(pathsFile, appHomePath);
    
    var runProcess = Process.Start(new ProcessStartInfo
    {
        FileName = "chmod",
        WorkingDirectory = appHomePath,
        Arguments = "+x gscli"
    });

    await runProcess!.WaitForExitAsync();
}

Console.WriteLine("Done, press any key to exit...");
Console.ReadKey();

