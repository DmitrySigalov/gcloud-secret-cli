using Google.Cloud.SecretManager.Client.Commands;
using Google.Cloud.SecretManager.Client.EnvironmentVariables;
using Google.Cloud.SecretManager.Client.GitHub;
using Google.Cloud.SecretManager.Client.GoogleCloud;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Google.Cloud.SecretManager.Client.VersionControl;
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
    await serviceProvider
        .GetRequiredService<IVersionControl>()
        .CheckVersionAsync(cts.Token);
    
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("GClod-Secrets-Cli"));

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
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Goodbye"));
}
