using System.ComponentModel.DataAnnotations;
using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.Google;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.Profiles.Helpers;
using Sharprompt;
using IProfileConfigProvider = GCloud.Secret.Client.Profiles.IProfileConfigProvider;

namespace GCloud.Secret.Client.Commands.Handlers;

public class ConfigProfileCommandHandler : ICommandHandler
{
    public const string COMMAND_NAME = "config-profile";
    
    private readonly IProfileConfigProvider _profileConfigProvider;
    private readonly ISecretManagerProvider _secretManagerProvider;

    private enum OperationEnum
    {
        New,
        Delete,
        Edit,
    }

    public ConfigProfileCommandHandler(IProfileConfigProvider profileConfigProvider,
        ISecretManagerProvider secretManagerProvider)
    {
        _profileConfigProvider = profileConfigProvider;
        _secretManagerProvider = secretManagerProvider;
    }

    public string CommandName => COMMAND_NAME;
    
    public string Description => "Profile(s) configuration";
    
    public async Task<ContinueStatusEnum> Handle(CommandState state, CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var profileDetails = GetProfileDetailsForConfiguration();

        if (profileDetails.Operation == OperationEnum.New)
        {
            SpinnerHelper.Run(
                () => _profileConfigProvider.Save(profileDetails.ProfileName, profileDetails.ProfileDo),
                $"Save new profile [{profileDetails.ProfileName}] configuration with default settings");
        }

        profileDetails.ProfileDo.PrintProfileConfig();

        if (profileDetails.Operation == OperationEnum.Delete)
        {
            SpinnerHelper.Run(
                () => _profileConfigProvider.Delete(profileDetails.ProfileName),
                $"Delete profile [{profileDetails.ProfileName}]");
            
            ConsoleHelper.WriteLineInfo($"DONE - Deleted profile [{profileDetails.ProfileName}]");

            return ContinueStatusEnum.Exit;
        }

        string lastSelectedOperationKey = null;        

        var saveOperationKey = "Save"; 

        while (lastSelectedOperationKey != saveOperationKey)
        {
            var manageOperationsLookup = new Dictionary<string, Func<Profiles.ProfileConfig, Task<(bool IsChanged, Profiles.ProfileConfig ProfileConfig)>>>
            {
                { saveOperationKey, pf => Save(profileDetails.ProfileName, pf) },
                { "Validate and get secret ids", pf => GetSecretIdsAsync(pf, false, cancellationToken) },
                { "Set project id", SetProjectId },
                { "Set advanced settings", SetAdvancedSettings },
                { "Reset to defaults", ResetDefaultSettings },
           };

            lastSelectedOperationKey = Prompt.Select(
                "Select operation",
                items: manageOperationsLookup.Keys,
                defaultValue: saveOperationKey);

            var operationFunction = manageOperationsLookup[lastSelectedOperationKey];

            (bool HasChanges, Profiles.ProfileConfig ProfileConfig) operationResult = await operationFunction(profileDetails.ProfileDo);

            if (operationResult.HasChanges)
            {
                profileDetails.ProfileDo = operationResult.ProfileConfig;
            }

            Console.WriteLine();
        }
        
        await GetSecretIdsAsync(profileDetails.ProfileDo, true, cancellationToken);

        ConsoleHelper.WriteLineInfo($"DONE - Configured valid profile [{profileDetails.ProfileName}]");
        Console.WriteLine();

        return ContinueStatusEnum.Exit;
    }
    
    private (OperationEnum Operation, string ProfileName, ProfileConfig ProfileDo) GetProfileDetailsForConfiguration()
    {
        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        var operation = OperationEnum.New;
        var profileName = "default";
        var profileDo = new ProfileConfig();

        if (profileNames.Any())
        {
            operation = Prompt.Select(
                "Select profile operation",
                items: new[] { OperationEnum.Edit, OperationEnum.New, OperationEnum.Delete },
                defaultValue: OperationEnum.Edit);
        }

        if (operation == OperationEnum.New)
        {
            profileName = Prompt.Input<string>(
                "Enter new profile name (project id) ",
                defaultValue: profileName,
                validators: new List<Func<object, ValidationResult>>
                {
                    check => ProfileNameValidationRule.Handle((string) check, profileNames),
                }).Trim();
            
            profileDo.ProjectId = profileName;
            
            return (operation, profileName, profileDo);
        }

        profileName =
            profileNames.Count == 1
                ? profileNames.Single()
                : Prompt.Select(
                    "Select profile",
                    items: profileNames);

        profileDo = SpinnerHelper.Run(
            () => _profileConfigProvider.GetByName(profileName),
            $"Read selected profile [{profileName}]");

        profileDo ??= new ProfileConfig(); 

        return (operation, profileName, profileDo);
    }

    private Task<(bool, ProfileConfig)> Save(string profileName, ProfileConfig profileConfig)
    {
        SpinnerHelper.Run(
            () => _profileConfigProvider.Save(profileName, profileConfig),
            $"Save profile [{profileName}] configuration new settings");
        
        return Task.FromResult((false, profileConfig));
    }
    
    private async Task<(bool, ProfileConfig)> GetSecretIdsAsync(ProfileConfig profileConfig, bool throwOnException, CancellationToken cancellationToken)
    {
        profileConfig.PrintProfileConfig();

        var secretIds = new HashSet<string>();
        
        try
        {
            secretIds = await _secretManagerProvider.GetSecretIdsAsync(
                profileConfig.ProjectId,
                cancellationToken);
        }
        catch (Exception e)
        {
            if (throwOnException)
            {
                throw;
            }
            
            ConsoleHelper.WriteLineError(e.Message);
        }

        if (secretIds.Any())
        {
            var secrets = profileConfig.BuildSecretDetails(secretIds);

            secrets.PrintSecretsMappingIdNames();
        }
        
        return (false, profileConfig);
    }

    private Task<(bool, ProfileConfig)> SetProjectId(ProfileConfig profileConfig)
    {
        var hasChanges = false;

        var newProjectId = Prompt.Input<string>(
            "Enter new project id",
            defaultValue: profileConfig.ProjectId);
        if (!string.IsNullOrEmpty(newProjectId))
        {
            profileConfig.ProjectId = newProjectId;
            hasChanges = true;
        }

        return Task.FromResult((hasChanges, profileConfig));
    }

    private Task<(bool, ProfileConfig)> SetAdvancedSettings(ProfileConfig profileConfig)
    {
        var hasChanges = false;

        var newEnvironmentVariablePrefix = Prompt.Input<string>(
            "Enter environment variable prefix",
            defaultValue: profileConfig.EnvironmentVariablePrefix);
        newEnvironmentVariablePrefix = newEnvironmentVariablePrefix?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(newEnvironmentVariablePrefix))
        {
            newEnvironmentVariablePrefix = null;
        }
        if (newEnvironmentVariablePrefix != profileConfig.EnvironmentVariablePrefix)
        {
            profileConfig.EnvironmentVariablePrefix = newEnvironmentVariablePrefix;
            hasChanges = true;
        }

        var newRemoveStartDelimiter = Prompt.Select(
            "Remove start delimiter",
            new[] { true, false, },
            defaultValue: profileConfig.RemoveStartDelimiter);
        if (newRemoveStartDelimiter != profileConfig.RemoveStartDelimiter)
        {
            profileConfig.RemoveStartDelimiter = newRemoveStartDelimiter;
            hasChanges = true;
        }

        var newConfigPathDelimiter = Prompt.Select(
            "Select secret path delimiter",
            new []
            {
                '/', '_', '\\',
                profileConfig.SecretPathDelimiter,
            }.Distinct(),
            defaultValue: profileConfig.SecretPathDelimiter);
        if (newConfigPathDelimiter != profileConfig.SecretPathDelimiter)
        {
            profileConfig.SecretPathDelimiter = newConfigPathDelimiter;
            hasChanges = true;
        }

        return Task.FromResult((hasChanges, profileConfig));
    }
    
    private Task<(bool, ProfileConfig)> ResetDefaultSettings(ProfileConfig profileConfig)
    {
        var newProfileConfig = new ProfileConfig();
        
        // Don't reset project id
        newProfileConfig.ProjectId = profileConfig.ProjectId ?? newProfileConfig.ProjectId;
        
        return Task.FromResult((true, newProfileConfig));
    }
}