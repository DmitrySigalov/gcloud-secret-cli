using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.GoogleCloud;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Grpc.Core;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers.Secrets;

public class DumpSecretsHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;
    private readonly ISecretManagerProvider _secretManagerProvider;

    public DumpSecretsHandler(IProfileConfigProvider profileConfigProvider,
        ISecretManagerProvider secretManagerProvider)
    {
        _profileConfigProvider = profileConfigProvider;
        _secretManagerProvider = secretManagerProvider;
    }

    public string CommandName => "dump-secrets";
    
    public string Description => "Get and save secrets from google";

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
                    items: profileNames);

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
            $"Dump {changesCounter} changes ({successCounter} values, {errorCounter} errors)",
            new[] { true, false, },
            defaultValue: errorCounter == 0);

        newSecrets.PrintSecretsMappingIdNamesAccessValues();

        if (!dumpSecrets)
        {
            ConsoleHelper.WriteLineNotification($"Not dumped {newSecrets.Count} secrets according to profile [{selectedProfileName}]");

            return;
        }

        _profileConfigProvider.DumpSecrets(selectedProfileName, newSecrets);

        ConsoleHelper.WriteLineInfo($"DONE - Dumped {newSecrets.Count} secrets according to profile [{selectedProfileName}]");
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