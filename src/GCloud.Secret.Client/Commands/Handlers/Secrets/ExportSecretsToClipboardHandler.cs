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
        
    public string ShortName => "es";
    
    public string Description => "Export secrets (copy json into clipboard) from the secrets dump";

    public Task<ContinueStatusEnum> Handle(CommandState commandState)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        if (string.IsNullOrEmpty(commandState.ProfileName))
        {
            var profileNames = SpinnerHelper.Run(
                _profileConfigProvider.GetNames,
                "Get profile names");

            if (profileNames.Any() == false)
            {
                ConsoleHelper.WriteLineError("Not found any profile");

                return Task.FromResult(ContinueStatusEnum.Exit);
            }

            commandState.ProfileName =Prompt.Select(
                "Select profile",
                items: profileNames);
        }

        if (commandState.SecretsDump == null)
        {
            commandState.SecretsDump = _profileConfigProvider.ReadSecrets(commandState.ProfileName);
            if (commandState.SecretsDump == null)
            {
                ConsoleHelper.WriteLineNotification($"Not found dump with accessed secret values according to profile [{commandState.ProfileName}]");

                return Task.FromResult(ContinueStatusEnum.Exit);
            }
        }

        var json = JsonSerializationHelper.Serialize(commandState.SecretsDump.ToSecretsDictionary());
        ClipboardService.SetText(json);
        Console.WriteLine(json);

        ConsoleHelper.WriteLineInfo($"DONE - Exported {commandState.SecretsDump.Count} secrets from dump according to profile [{commandState.ProfileName}]");

        return Task.FromResult(ContinueStatusEnum.Exit);
    }
}