using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.EnvironmentVariables;
using GCloud.Secret.Client.Google;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.Profiles.Helpers;
using Grpc.Core;
using Sharprompt;

namespace GCloud.Secret.Client.Commands.Handlers.Secrets;

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
        
    public string ShortName => "gs";
    
    public string Description => "Get and save/dump secrets from google";

    public async Task<ContinueStatusEnum> Handle(CommandState commandState)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var currentEnvironmentDescriptor = _environmentVariablesProvider.Get() ?? new EnvironmentDescriptor();

        if (string.IsNullOrEmpty(commandState.ProfileName))
        {
            var profileNames = SpinnerHelper.Run(
                _profileConfigProvider.GetNames,
                "Get profile names");

            if (profileNames.Any() == false)
            {
                ConsoleHelper.WriteLineError("Not found any profile");

                return ContinueStatusEnum.Exit;
            }

            commandState.ProfileName =
                profileNames.Count == 1
                    ? profileNames.Single()
                    : Prompt.Select(
                        "Select profile",
                        items: profileNames,
                        defaultValue: currentEnvironmentDescriptor.ProfileName);
        }
        
        if (commandState.ProfileConfig == null)
        {
            commandState.ProfileConfig = SpinnerHelper.Run(
                () => _profileConfigProvider.GetByName(commandState.ProfileName),
                $"Read profile [{commandState.ProfileName}]");

            if (commandState.ProfileConfig == null)
            {
                ConsoleHelper.WriteLineError($"Not found profile [{commandState.ProfileName}]");

                return ContinueStatusEnum.Exit;
            }

            commandState.ProfileConfig.PrintProfileConfig();
        }


        var newSecretIds = await GetSecretIdsAsync(commandState);

        commandState.SecretsDump = commandState.ProfileConfig.BuildSecretDetails(newSecretIds);
        if (!commandState.SecretsDump.Any())
        {
            ConsoleHelper.WriteLineNotification("Nothing data to synchronize");

            return ContinueStatusEnum.Exit;
        }

        var oldSecrets = _profileConfigProvider.ReadSecrets(commandState.ProfileName);

        var changesCounter = await ProgressGetSecretLatestValuesAsync(commandState.ProfileConfig.ProjectId,
            commandState.SecretsDump, oldSecrets,
            commandState.CancellationToken);

        var successCounter = commandState.SecretsDump.Count(x => x.Value.AccessStatusCode == StatusCode.OK);
        var errorCounter = commandState.SecretsDump.Count(x => x.Value.AccessStatusCode != StatusCode.OK);

        if (successCounter == 0)
        {
            ConsoleHelper.WriteLineNotification(
                $"NO DATA - Not retrieved any valid secret value ({errorCounter} errors)");
            
            return ContinueStatusEnum.Exit;
        }

        commandState.SecretsDump.PrintSecretsMappingIdNamesAccessValues();

        if (changesCounter == 0)
        {
            ConsoleHelper.WriteLineInfo(
                $"DUMP NO CHANGES - Fully valid synchronized data ({successCounter} values, {errorCounter} errors)");
        }
        else
        {
            _profileConfigProvider.DumpSecrets(commandState.ProfileName, commandState.SecretsDump);

            ConsoleHelper.WriteLineInfo($"DONE - Saved/dumped {commandState.SecretsDump.Count} secrets ({successCounter} values, {errorCounter} errors) according to profile [{commandState.ProfileName}]");
        }
        
        return ContinueStatusEnum.SetEnvironment;
    }

    private async Task<HashSet<string>> GetSecretIdsAsync(CommandState commandState)
    {
        try
        {
            return await _secretManagerProvider.GetSecretIdsAsync(
                commandState.ProfileConfig.ProjectId,
                commandState.CancellationToken);
        }
        catch (Exception e)
        {
            ConsoleHelper.WriteLineError("Error on attempt to get secret ids:");
            ConsoleHelper.WriteLineWarn(e.Message);
        }
        
        return new HashSet<string>();
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
                syncStatus = "UPDATED";
                if (oldSecrets?.ContainsKey(secretDetails.Key) != true)
                {
                    syncStatus = "ADDED";
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