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
    
    public Task<ContinueStatusEnum> Handle(CommandState commandState, CancellationToken cancellationToken)
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

        var selectedProfileDo = SpinnerHelper.Run(
            () => _profileConfigProvider.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");
        if (selectedProfileDo == null)
        {
            ConsoleHelper.WriteLineError($"Not found profile [{selectedProfileName}]");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        var newSecrets = selectedProfileDo.BuildSecretDetails(
            importedData.Secrets.Keys.ToHashSet());

        foreach (var secret in importedData.Secrets)
        {
            var newSecret = newSecrets[secret.Key];

            newSecret.AccessStatusCode = StatusCode.OK;
            newSecret.DecodedValue = secret.Value;
        }

        _profileConfigProvider.DumpSecrets(selectedProfileName, newSecrets);

        newSecrets.PrintSecretsMappingIdNamesAccessValues();
        
        ConsoleHelper.WriteLineInfo($"DONE - Imported/saved/dumped {newSecrets.Count} secrets according to profile [{selectedProfileName}]");

        return Task.FromResult(ContinueStatusEnum.Exit);
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