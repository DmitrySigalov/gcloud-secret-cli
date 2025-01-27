using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.Profiles.Helpers;
using Grpc.Core;
using Sharprompt;
using TextCopy;

namespace GCloud.Secret.Client.Commands.Handlers.Secrets;

public class ImportSecretsFromClipboardHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    public ImportSecretsFromClipboardHandler(
        IProfileConfigProvider profileConfigProvider)
    {
        _profileConfigProvider = profileConfigProvider;
    }
    
    public string CommandName => "import-secrets";
        
    public string ShortName => "is";
    
    public string Description => "Import secrets (json from the clipboard)";
    
    public Task<ContinueStatusEnum> Handle(CommandState commandState)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var importedData = GetSecretsFromClipboard();

        if (importedData.Secrets?.Any() != true)
        {
            ConsoleHelper.WriteLineInfo("No imported data");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }
        
        ConsoleHelper.WriteLineInfo($"Imported json:");
        Console.WriteLine(importedData.ClipboardText);
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

            commandState.ProfileName = Prompt.Select(
                "Select profile",
                items: profileNames);
        }

        if (commandState.ProfileConfig == null)
        {
            commandState.ProfileConfig = SpinnerHelper.Run(
                () => _profileConfigProvider.GetByName(commandState.ProfileName),
                $"Read profile [{commandState.ProfileName}]");
            if (commandState.ProfileConfig == null)
            {
                ConsoleHelper.WriteLineError($"Not found profile [{commandState.ProfileName}]");

                return Task.FromResult(ContinueStatusEnum.Exit);
            }
        }

        commandState.SecretsDump = commandState.ProfileConfig.BuildSecretDetails(
            importedData.Secrets.Keys.ToHashSet());

        foreach (var secret in importedData.Secrets)
        {
            var newSecret = commandState.SecretsDump[secret.Key];

            newSecret.AccessStatusCode = StatusCode.OK;
            newSecret.DecodedValue = secret.Value;
        }

        _profileConfigProvider.DumpSecrets(commandState.ProfileName, commandState.SecretsDump);

        commandState.SecretsDump.PrintSecretsMappingIdNamesAccessValues();
        
        ConsoleHelper.WriteLineInfo($"DONE - Imported/saved/dumped {commandState.SecretsDump.Count} secrets according to profile [{commandState.ProfileName}]");

        return Task.FromResult(ContinueStatusEnum.SetEnvironment);
    }

    private (string ClipboardText, Dictionary<string, string> Secrets) GetSecretsFromClipboard()
    {
        var clipboardText = ClipboardService.GetText();

        try
        {
            var secrets = JsonSerializationHelper.Deserialize<Dictionary<string, string>>(clipboardText);
            
            return (clipboardText, secrets);
        }
        catch (Exception e)
        {
            ConsoleHelper.WriteLineError("Invalid or missing json: ");

            Console.WriteLine(e.Message);
            
            Console.WriteLine();
        }
        
        return (clipboardText, null);
    }
}