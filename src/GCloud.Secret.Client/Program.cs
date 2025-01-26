using GCloud.Secret.Client.Commands;
using GCloud.Secret.Client.EnvironmentVariables;
using GCloud.Secret.Client.GitHub;
using GCloud.Secret.Client.Google;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.UserRuntime;
using GCloud.Secret.Client.VersionControl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", false)
    .Build();

var services = new ServiceCollection();

services
    .AddSingleton<IConfiguration>(configuration)
    .AddLogging(builder =>
    {
        builder.ClearProviders();

        builder
            .SetMinimumLevel(LogLevel.Error)
            .AddSimpleConsole();
    });

services
    .AddCommands()
    .AddGoogleCloudServices()
    .AddUserRuntimeServices(args)
    .AddEnvironmentVariablesServices()
    .AddProfileServices()
    .AddVersionControlServices()
    .AddGitHubServices();

var serviceProvider = services.BuildServiceProvider();

try
{
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("GCloud-Secret-Cli"));

    var cliHandler = serviceProvider
        .GetRequiredService<CommandSelector>()
        .Get();

    await cliHandler.Handle(cts.Token);
}
catch (Exception e)
{
    serviceProvider
        .GetRequiredService<ILogger<Program>>()
        .LogError(e, "An error has occurred");
}
finally
{
    Thread.Sleep(1000);
    
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Goodbye"));
    
    await serviceProvider
        .GetRequiredService<IVersionControl>()
        .CheckVersionAsync(cts.Token);
}

