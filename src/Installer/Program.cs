using Installer;
using Installer.Installers;
using Microsoft.Extensions.Configuration;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var installer = InstallerFactory.Get();

var installerArgs = new InstallerArgs
{
    Configuration = new ConfigurationBuilder()
        .AddCommandLine(args)
        .Build(),
    CancellationToken = cts.Token,
};

Console.WriteLine($"Installing {ApplicationSettings.AppName}...");

await installer.RunAsync(installerArgs);

Console.WriteLine("Done, press any key to exit...");
Console.ReadKey();

// ...................
// TODO: Delete
// ...................

//
//
//
// if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
//     !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
// {
//     throw new NotSupportedException($"Not supported {RuntimeInformation.RuntimeIdentifier}");
// }
//
// var configuration = new ConfigurationBuilder()
//     .AddCommandLine(args)
//     .Build();
//
// var appHomePath = InstallerHelper.GetAppHomeDirectory(configuration);
//
// Console.WriteLine("Installing...");
//
// var buildWorkingDirectory = InstallerHelper.GetBuildWorkingDirectory(configuration);
//
// var process = Process.Start(new ProcessStartInfo
// {
//     FileName = "dotnet",
//     WorkingDirectory = buildWorkingDirectory,
//     Arguments =
//         $"publish {InstallerHelper.ProjectPath} --output {appHomePath} --source https://api.nuget.org/v3/index.json --configuration Release --verbosity quiet /property:WarningLevel=0"
// });
//
// await process!.WaitForExitAsync();
//
// Console.WriteLine("Adding app to machine path...");
//
// if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
// {
//     var name = "PATH";
//     var scope = EnvironmentVariableTarget.Machine;
//     var oldValue = Environment.GetEnvironmentVariable(name, scope);
//
//     if (InstallerHelper.ShouldUpdateWindowsPaths(oldValue!, appHomePath))
//     {
//         var newPaths = InstallerHelper.GetNewWindowsPaths(oldValue!, appHomePath);
//         Environment.SetEnvironmentVariable(name, newPaths, scope);
//     }
//
//     Console.WriteLine("Adding app to machine path...");
// }
// else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
// {
//     var pathsFile = $"/etc/paths.d/{InstallerHelper.AppName}";
//
//     await File.WriteAllTextAsync(pathsFile, appHomePath);
//     
//     var runProcess = Process.Start(new ProcessStartInfo
//     {
//         FileName = "chmod",
//         WorkingDirectory = appHomePath,
//         Arguments = "+x gscli"
//     });
//
//     await runProcess!.WaitForExitAsync();
// }
//
// Console.WriteLine("Done, press any key to exit...");
// Console.ReadKey();
//
