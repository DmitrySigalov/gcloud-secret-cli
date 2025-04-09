using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.Google;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.Profiles.Helpers;
using Sharprompt;
using IProfileConfigProvider = GCloud.Secret.Client.Profiles.IProfileConfigProvider;

namespace GCloud.Secret.Client.Commands.Handlers.ProfileConfiguration;

public class EditProfileCommandHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;
    private readonly ISecretManagerProvider _secretManagerProvider;

    public EditProfileCommandHandler(IProfileConfigProvider profileConfigProvider,
        ISecretManagerProvider secretManagerProvider)
    {
        _profileConfigProvider = profileConfigProvider;
        _secretManagerProvider = secretManagerProvider;
    }

    public string CommandName => "edit-profile";
            
    public string ShortName => "ep";

    public string Description => "Edit profile configuration";
    
    public async Task<ContinueStatusEnum> Handle(CommandState commandState)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        GetProfileDetailsForConfiguration(commandState);
        if (commandState.ProfileConfig == null)
        {
            ConsoleHelper.WriteLineError($"Not found profile [{commandState.ProfileName}]");

            return ContinueStatusEnum.Exit;
        }

        string lastSelectedOperationKey = null;        

        var saveOperationKey = "Save"; 

        while (lastSelectedOperationKey != saveOperationKey)
        {
            var manageOperationsLookup = new Dictionary<string, Func<CommandState, Task>>
            {
                { saveOperationKey, Save },
                { "Validate / get secret ids", ValidateGetSecretIdsAsync },
                { "Set project id", SetProjectId },
                { "Environment variables naming settings", SetEnvironmentVariableSettings },
                { "Reset to defaults", ResetDefaultSettings },
           };

            lastSelectedOperationKey = Prompt.Select(
                "Select operation",
                items: manageOperationsLookup.Keys,
                defaultValue: saveOperationKey);

            var operationFunction = manageOperationsLookup[lastSelectedOperationKey];

            await operationFunction(commandState);

            Console.WriteLine();
        }
        
        ConsoleHelper.WriteLineInfo($"DONE - Configured profile [{commandState.ProfileName}]");
        Console.WriteLine();

        return ContinueStatusEnum.Exit;
    }
    
    private void GetProfileDetailsForConfiguration(
        CommandState commandState)
    {
        if (string.IsNullOrEmpty(commandState.ProfileName))
        {
            var profileNames = SpinnerHelper.Run(
                _profileConfigProvider.GetNames,
                "Get profile names");

            commandState.ProfileName = Prompt.Select(
                "Select profile",
                items: profileNames);
        }

        if (commandState.ProfileConfig == null)
        {
            commandState.ProfileConfig = SpinnerHelper.Run(
                () => _profileConfigProvider.GetByName(commandState.ProfileName),
                $"Read selected profile [{commandState.ProfileName}]");
            
            if (commandState.ProfileConfig == null)
            {
                ConsoleHelper.WriteLineError($"Not found profile [{commandState.ProfileName}]");
            }
        }
    }

    private async Task Save(CommandState commandState)
    {
        SpinnerHelper.Run(
            () => _profileConfigProvider.Save(commandState.ProfileName, commandState.ProfileConfig),
            $"Save profile [{commandState.ProfileName}] configuration new settings");

        await ValidateGetSecretIdsAsync(commandState);
    }
    
    private async Task ValidateGetSecretIdsAsync(CommandState commandState)
    {
        commandState.ProfileConfig.PrintProfileConfig();

        var secretIds = new HashSet<string>();
        
        try
        {
            secretIds = await _secretManagerProvider.GetSecretIdsAsync(
                commandState.ProfileConfig.ProjectId,
                commandState.CancellationToken);
        }
        catch (Exception e)
        {
            ConsoleHelper.WriteLineError("Error on attempt to get secret ids:");
            ConsoleHelper.WriteLineWarn(e.Message);
        }

        if (secretIds.Any())
        {
            var secrets = commandState.ProfileConfig.BuildSecretDetails(secretIds);

            secrets.PrintSecretsMappingIdNames();
        }
    }

    private Task SetProjectId(CommandState commandState)
    {
        var hasChanges = false;

        var newProjectId = Prompt.Input<string>(
            "Enter new project id",
            defaultValue: commandState.ProfileConfig.ProjectId);
        if (!string.IsNullOrEmpty(newProjectId))
        {
            commandState.ProfileConfig.ProjectId = newProjectId;
            hasChanges = true;
        }

        return Task.CompletedTask;
    }

    private Task SetEnvironmentVariableSettings(CommandState commandState)
    {
        var newEnvironmentVariablePrefix = Prompt.Input<string>(
            "Enter environment variable prefix",
            defaultValue: commandState.ProfileConfig.EnvironmentVariablePrefix);
        newEnvironmentVariablePrefix = newEnvironmentVariablePrefix?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(newEnvironmentVariablePrefix))
        {
            newEnvironmentVariablePrefix = null;
        }
        if (newEnvironmentVariablePrefix != commandState.ProfileConfig.EnvironmentVariablePrefix)
        {
            commandState.ProfileConfig.EnvironmentVariablePrefix = newEnvironmentVariablePrefix;
        }

        var newRemoveStartDelimiter = Prompt.Select(
            "Remove start delimiter",
            new[] { true, false, },
            defaultValue: commandState.ProfileConfig.RemoveStartDelimiter);
        if (newRemoveStartDelimiter != commandState.ProfileConfig.RemoveStartDelimiter)
        {
            commandState.ProfileConfig.RemoveStartDelimiter = newRemoveStartDelimiter;
        }

        return Task.CompletedTask;
    }
    
    private Task ResetDefaultSettings(CommandState commandState)
    {
        var newProfileConfig = new ProfileConfig();
        
        // Don't reset project id
        newProfileConfig.ProjectId = commandState.ProfileConfig.ProjectId ?? newProfileConfig.ProjectId;
        
        commandState.ProfileConfig = newProfileConfig;
        
        return Task.CompletedTask;
    }
}