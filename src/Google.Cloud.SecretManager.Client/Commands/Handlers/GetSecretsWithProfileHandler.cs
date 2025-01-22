using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.GCloud;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class GetSecretsWithProfileHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;
    private readonly ISecretManagerProvider _secretManagerProvider;

    public GetSecretsWithProfileHandler(IProfileConfigProvider profileConfigProvider,
        ISecretManagerProvider secretManagerProvider)
    {
        _profileConfigProvider = profileConfigProvider;
        _secretManagerProvider = secretManagerProvider;
    }

    public string CommandName => "get-secrets";
    
    public string Description => "Get (export) secrets with profile configuration";
    
    public async Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleHelper.WriteLineError("No found any profile");

            return;
        }

        var selectedProfileName = 
            profileNames.Count == 1
                ? profileNames.Single()
                : Prompt.Select(
                    "Select profile",
                    items: profileNames,
                    defaultValue: profileNames.First());

        var selectedProfileDo = SpinnerHelper.Run(
            () => _profileConfigProvider.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");

        if (selectedProfileDo == null)
        {
            ConsoleHelper.WriteLineError($"No found profile [{selectedProfileName}]");

            return;
        }

        selectedProfileDo.PrintProfileConfig();

        var oldSecrets = _profileConfigProvider.ReadSecrets(selectedProfileName);
        
        var secretIds = await _secretManagerProvider.GetSecretIdsAsync(
            selectedProfileDo.ProjectId, 
            cancellationToken);
        
        var newSecrets = selectedProfileDo.BuildSecretDetails(secretIds);
        
        newSecrets.PrintSecretsMappingIdNames();

        var changesCounter = await newSecrets.PrintProgressGetSecretLatestDiffValuesAsync(
            secret => _secretManagerProvider.ApplySecretLatestValueAsync(
                selectedProfileDo.ProjectId,
                secret.Key, secret.Value, 
                cancellationToken),
            oldSecrets,
            cancellationToken);

        newSecrets.RemoveSecretsWithNoValue();

        if (changesCounter > 0)
        {
            var dumpSecrets = Prompt.Select(
                $"Dump {changesCounter} change(s)",
                new[] { true, false, },
                defaultValue: true);

            if (dumpSecrets)
            {
                _profileConfigProvider.DumpSecrets(selectedProfileName, newSecrets);
            }
        }

        ConsoleHelper.WriteLineInfo($"DONE - Selected profile [{selectedProfileName}], {newSecrets.Count} retrieved secrets, {changesCounter} change(s)");
    }
}