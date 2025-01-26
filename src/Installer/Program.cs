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
