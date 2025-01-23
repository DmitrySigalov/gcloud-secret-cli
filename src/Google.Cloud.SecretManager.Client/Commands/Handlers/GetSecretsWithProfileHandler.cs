using ConsoleTables;
using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.GoogleCloud;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Grpc.Core;
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
    
    public string Description => "Get secrets with profile configuration";
    
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

        var changesCounter = await ProgressGetSecretLatestValuesAsync(selectedProfileDo.ProjectId,
            newSecrets, oldSecrets,
            cancellationToken);

        var successCounter = newSecrets.Count(x => x.Value.AccessStatusCode == StatusCode.OK);
        var errorCounter = newSecrets.Count(x => x.Value.AccessStatusCode != StatusCode.OK);

        if (changesCounter > 0)
        {
            var dumpSecrets = Prompt.Select(
                $"Dump {changesCounter} change(s) ({successCounter} value(s), {errorCounter} error(s))",
                new[] { true, false, },
                defaultValue: errorCounter == 0);

            if (dumpSecrets)
            {
                _profileConfigProvider.DumpSecrets(selectedProfileName, newSecrets);
            }
        }
        else if (newSecrets.Any())
        {
            ConsoleHelper.WriteLineWarn($"Fully valid synchronized data ({successCounter} value(s), {errorCounter} error(s))");
        }
        else
        {
            ConsoleHelper.WriteLineNotification("Nothing data to synchronize");
        }

        ConsoleHelper.WriteLineInfo($"DONE - Selected profile [{selectedProfileName}], {newSecrets.Count} retrieved secrets");
    }
    
    private async Task<int> ProgressGetSecretLatestValuesAsync(string projectId,
        IDictionary<string, SecretDetails> newSecrets,
        IDictionary<string, SecretDetails> oldSecrets,
        CancellationToken cancellationToken)
    {
        var changesCounter = 0;
        
        ConsoleHelper.WriteLineInfo("Access to latest secret values...");

        var table = new ConsoleTable("sync-status", "secret-id", "decoded value/error");
        table.Write(Format.Minimal);

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
            var syncStatus = "PASS";

            if (secretDetails.Value.AccessStatusCode != StatusCode.OK)
            {
                writeAction = ConsoleHelper.WriteError;
                syncStatus = secretDetails.Value.AccessStatusCode.ToString();
            }
            else if (hasChanges)
            {
                writeAction = ConsoleHelper.WriteWarn;
                syncStatus = "CHANGED";
                if (oldSecrets?.ContainsKey(secretDetails.Key) != true)
                {
                    syncStatus = "NEW";
                }
            }
            writeAction($"{syncStatus}\t");

            ConsoleHelper.WriteNotification($"{secretDetails.Key}\t");
            
            if (secretDetails.Value.AccessStatusCode == StatusCode.OK)
            {
                Console.Write($"{secretDetails.Value.DecodedValue}");
            }
            
            if (hasChanges)
            {
                ++changesCounter;
            }

            Console.WriteLine();
        }

        foreach (var oldSecretDetails in oldSecrets ?? new Dictionary<string, SecretDetails>())
        {
            if (!newSecrets.ContainsKey(oldSecretDetails.Key))
            {
                ConsoleHelper.WriteWarn("DELETED\t");
                ConsoleHelper.WriteNotification($"{oldSecretDetails.Key}\t");
                Console.Write($"{oldSecretDetails.Value.DecodedValue}");
                ++changesCounter;
            }
        }

        Console.WriteLine();

        return changesCounter;
    }
}