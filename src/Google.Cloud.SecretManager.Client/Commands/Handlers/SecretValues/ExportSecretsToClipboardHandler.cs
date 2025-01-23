using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Sharprompt;
using TextCopy;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers.SecretValues;

public class ExportSecretsToClipboardHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    public ExportSecretsToClipboardHandler(
        IProfileConfigProvider profileConfigProvider)
    {
        _profileConfigProvider = profileConfigProvider;
    }
    
    public string CommandName => "export-secrets";
    
    public string Description => "Export secrets (copy into clipboard)";

    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleHelper.WriteLineError("No found any profile");

            return Task.CompletedTask;
        }

        var selectedProfileName =
            profileNames.Count == 1
                ? profileNames.Single()
                : Prompt.Select(
                    "Select profile",
                    items: profileNames);

        var currentSecrets = _profileConfigProvider
            .ReadSecrets(selectedProfileName)
            ?.ToDictionary();
        if (currentSecrets?.Any() != true)
        {
            ConsoleHelper.WriteLineNotification($"Not valid secrets in the dump according to profile [{selectedProfileName}]");

            return Task.CompletedTask;

        }

        var json = JsonSerializationHelper.Serialize(currentSecrets.ToSecretsDictionary());
        ClipboardService.SetText(json);
        Console.WriteLine(json);

        ConsoleHelper.WriteLineInfo($"DONE - Exported {currentSecrets.Count} secrets from dump according to profile [{selectedProfileName}]");

        return Task.CompletedTask;
    }
}