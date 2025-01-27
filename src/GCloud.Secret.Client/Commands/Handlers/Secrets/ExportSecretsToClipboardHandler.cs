using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.Profiles.Helpers;
using Sharprompt;
using TextCopy;

namespace GCloud.Secret.Client.Commands.Handlers.Secrets;

public class ExportSecretsToClipboardHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    public ExportSecretsToClipboardHandler(
        IProfileConfigProvider profileConfigProvider)
    {
        _profileConfigProvider = profileConfigProvider;
    }
    
    public string CommandName => "export-secrets";
    
    public string Description => "Export secrets (copy json into clipboard) from the secrets dump";

    public Task<ContinueStatusEnum> Handle(CommandState commandState, CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleHelper.WriteLineError("Not found any profile");

            return Task.FromResult(ContinueStatusEnum.Exit);
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
            ConsoleHelper.WriteLineNotification($"Not found any valid secret value in the dump according to profile [{selectedProfileName}]");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        var json = JsonSerializationHelper.Serialize(currentSecrets.ToSecretsDictionary());
        ClipboardService.SetText(json);
        Console.WriteLine(json);

        ConsoleHelper.WriteLineInfo($"DONE - Exported {currentSecrets.Count} secrets from dump according to profile [{selectedProfileName}]");

        return Task.FromResult(ContinueStatusEnum.Exit);
    }
}