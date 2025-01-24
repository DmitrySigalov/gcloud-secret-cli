using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.EnvironmentVariables;
using Google.Cloud.SecretManager.Client.GoogleCloud;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Grpc.Core;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers.Secrets;

public class GetSecretsHandler : ICommandHandler
{
    public const string COMMAND_NAME = "get-secrets";
    
    private readonly IProfileConfigProvider _profileConfigProvider;
    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
    private readonly ISecretManagerProvider _secretManagerProvider;

    public GetSecretsHandler(IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ISecretManagerProvider secretManagerProvider)
    {
        _profileConfigProvider = profileConfigProvider;
        _environmentVariablesProvider = environmentVariablesProvider;
        _secretManagerProvider = secretManagerProvider;
    }

    public string CommandName => COMMAND_NAME;
    
    public string Description => "Get and save/dump secrets from google";

    public async Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleHelper.WriteLineError("Not found any profile");

            return;
        }

        var currentEnvironmentDescriptor = _environmentVariablesProvider.Get() ?? new EnvironmentDescriptor();

        var selectedProfileName =
            profileNames.Count == 1
                ? profileNames.Single()
                : Prompt.Select(
                    "Select profile",
                    items: profileNames,
                    defaultValue: currentEnvironmentDescriptor.ProfileName);

        var selectedProfileDo = SpinnerHelper.Run(
            () => _profileConfigProvider.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");

        if (selectedProfileDo == null)
        {
            ConsoleHelper.WriteLineError($"Not found profile [{selectedProfileName}]");

            return;
        }

        selectedProfileDo.PrintProfileConfig();

        var oldSecrets = _profileConfigProvider.ReadSecrets(selectedProfileName);

        var secretIds = await _secretManagerProvider.GetSecretIdsAsync(
            selectedProfileDo.ProjectId,
            cancellationToken);

        var newSecrets = selectedProfileDo.BuildSecretDetails(secretIds);

        if (!newSecrets.Any())
        {
            ConsoleHelper.WriteLineNotification("Nothing data to synchronize");

            return;
        }

        var changesCounter = await ProgressGetSecretLatestValuesAsync(selectedProfileDo.ProjectId,
            newSecrets, oldSecrets,
            cancellationToken);

        var successCounter = newSecrets.Count(x => x.Value.AccessStatusCode == StatusCode.OK);
        var errorCounter = newSecrets.Count(x => x.Value.AccessStatusCode != StatusCode.OK);

        if (changesCounter == 0)
        {
            newSecrets.PrintSecretsMappingIdNamesAccessValues();

            ConsoleHelper.WriteLineInfo(
                $"NO CHANGES - Fully valid synchronized data ({successCounter} values, {errorCounter} errors)");
            
            return;
        }

        var dumpSecrets = Prompt.Select(
            $"Create and dump {changesCounter} changes ({successCounter} values, {errorCounter} errors)",
            new[] { true, false, },
            defaultValue: errorCounter == 0);

        newSecrets.PrintSecretsMappingIdNamesAccessValues();

        if (!dumpSecrets)
        {
            return;
        }

        _profileConfigProvider.DumpSecrets(selectedProfileName, newSecrets);

        ConsoleHelper.WriteLineInfo($"DONE - Saved/dumped {newSecrets.Count} secrets according to profile [{selectedProfileName}]");
    }

    private async Task<int> ProgressGetSecretLatestValuesAsync(string projectId,
        IDictionary<string, SecretDetails> newSecrets,
        IDictionary<string, SecretDetails> oldSecrets,
        CancellationToken cancellationToken)
    {
        var changesCounter = 0;
        
        ConsoleHelper.WriteLineInfo("Access to latest secret values...");

        foreach (var secretDetails in newSecrets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _secretManagerProvider.ApplySecretLatestValueAsync(
                projectId,
                secretDetails.Key, secretDetails.Value,
                cancellationToken);
            
            var hasChanges = oldSecrets == null || 
                             !oldSecrets.TryGetValue(secretDetails.Key, out var oldSecret) ||
                             secretDetails.Value.DecodedValue != oldSecret.DecodedValue;

            Action<string> writeAction = ConsoleHelper.WriteInfo;
            
            var syncStatus = secretDetails.Value.AccessStatusCode.ToString();

            if (secretDetails.Value.AccessStatusCode != StatusCode.OK)
            {
                writeAction = ConsoleHelper.WriteError;
            }
            else if (hasChanges)
            {
                writeAction = ConsoleHelper.WriteNotification;
                syncStatus = "CHANGED";
                if (oldSecrets?.ContainsKey(secretDetails.Key) != true)
                {
                    syncStatus = "NEW";
                }
            }
            writeAction($"{syncStatus}\t");

            Console.WriteLine($"{secretDetails.Key}\t");
            
            if (hasChanges)
            {
                ++changesCounter;
            }
        }

        foreach (var oldSecretDetails in oldSecrets ?? new Dictionary<string, SecretDetails>())
        {
            if (!newSecrets.ContainsKey(oldSecretDetails.Key))
            {
                ConsoleHelper.WriteNotification("DELETED\t");
                Console.WriteLine($"{oldSecretDetails.Key}\t");
                ++changesCounter;
            }
        }

        Console.WriteLine();

        return changesCounter;
    }
}