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

        var lastActiveProfileName = _profileConfigProvider.ActiveName;
        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleHelper.WriteLineNotification($"Current active profile is [{lastActiveProfileName}]");
        }

        var selectedProfileName = 
            profileNames.Count == 1
                ? profileNames.Single()
                : Prompt.Select(
                    "Select profile",
                    items: profileNames,
                    defaultValue: lastActiveProfileName);

        var selectedProfileDo = SpinnerHelper.Run(
            () => _profileConfigProvider.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");

        if (selectedProfileDo == null)
        {
            ConsoleHelper.WriteLineError($"No found profile [{selectedProfileName}]");

            return;
        }

        selectedProfileDo.PrintProfileConfig();
        
        var secretIds = await _secretManagerProvider.GetSecretIdsAsync(
            selectedProfileDo.ProjectId, 
            cancellationToken);
        
        var secrets = selectedProfileDo.BuildSecretDetails(secretIds);
        
        secrets.PrintSecretsMappingIdNames();

        await secrets.PrintProgressGetSecretLatestValuesAsync(
            secret => _secretManagerProvider.ApplySecretLatestValueAsync(
                selectedProfileDo.ProjectId,
                secret.Key, secret.Value, 
                cancellationToken));

        ConsoleHelper.WriteLineInfo($"DONE - Selected profile [{selectedProfileName}]");
    }
}