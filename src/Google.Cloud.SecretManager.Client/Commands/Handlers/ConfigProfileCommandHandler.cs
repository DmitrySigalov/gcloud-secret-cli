using System.ComponentModel.DataAnnotations;
using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Sharprompt;
using TextCopy;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class ConfigProfileCommandHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;
    
    private enum OperationEnum
    {
        New,
        Delete,
        Edit,
    }

    public ConfigProfileCommandHandler(
        IProfileConfigProvider profileConfigProvider)
    {
        _profileConfigProvider = profileConfigProvider;
    }

    public string CommandName => "config";
    
    public string Description => "Profile(s) configuration";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var lastActiveProfileName = _profileConfigProvider.ActiveName;
        var lastActiveProfileDo = default(ProfileConfig);
        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            _profileConfigProvider.ActiveName = null;

            lastActiveProfileDo = _profileConfigProvider.GetByName(lastActiveProfileName);
        }
        
        var profileDetails = GetProfileDetailsForConfiguration(lastActiveProfileName, lastActiveProfileDo);

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

            return Task.CompletedTask;
        }

        string lastSelectedOperationKey = null;        

        var completeExitOperationKey = "Complete/exit configuration"; 

        while (lastSelectedOperationKey != completeExitOperationKey)
        {
            var manageOperationsLookup = new Dictionary<string, Func<ProfileConfig, (bool IsChanged, ProfileConfig ProfileConfig)>>
            {
                { completeExitOperationKey, Exit },
                { "Validate Profile", ValidateProfile },
                { "Set Project Id", SetProjectId },
                { "Import json configuration (paste from clipboard)", ImportJsonConfigurationFromClipboard },
                { "Export json configuration (copy into clipboard)", ExportJsonConfigurationIntoClipboard },
                { "Default", SetDefaultConfiguration },
           };

            lastSelectedOperationKey = Prompt.Select(
                "Select operation",
                items: manageOperationsLookup.Keys,
                defaultValue: completeExitOperationKey);

            var operationFunction = manageOperationsLookup[lastSelectedOperationKey];

            (bool HasChanges, ProfileConfig ProfileConfig) operationResult = operationFunction(profileDetails.ProfileDo);

            if (operationResult.HasChanges)
            {
                SpinnerHelper.Run(
                    () => _profileConfigProvider.Save(profileDetails.ProfileName, operationResult.ProfileConfig),
                    $"Save profile [{profileDetails.ProfileName}] configuration new settings");
            
                operationResult.ProfileConfig.PrintProfileConfig();
                
                profileDetails.ProfileDo = operationResult.ProfileConfig;
            }
        }

        ConsoleHelper.WriteLineInfo($"DONE - Profile [{profileDetails.ProfileName}] configuration");
        Console.WriteLine();

        return Task.CompletedTask;
    }
    
    private (OperationEnum Operation, string ProfileName, ProfileConfig ProfileDo) GetProfileDetailsForConfiguration(
        string lastActiveProfileName,
        ProfileConfig currentProfileDo)
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
                    (check) => ProfileNameValidationRule.Handle((string) check, profileNames),
                }).Trim();
            
            profileDo.ProjectId = profileName;
            
            return (operation, profileName, profileDo);
        }

        profileName =
            profileNames.Count == 1
                ? profileNames.Single()
                : Prompt.Select(
                    "Select profile",
                    items: profileNames,
                    defaultValue: lastActiveProfileName);

        profileDo = lastActiveProfileName == profileName
            ? currentProfileDo
            : SpinnerHelper.Run(
                () => _profileConfigProvider.GetByName(profileName),
                $"Read selected profile [{profileName}]");

        profileDo ??= new ProfileConfig(); 

        return (operation, profileName, profileDo);
    }
    
    private (bool, ProfileConfig) Exit(ProfileConfig profileConfig) => (false, profileConfig);
    
    private (bool, ProfileConfig) ValidateProfile(ProfileConfig profileConfig)
    {
        // TODO
        
        return (false, profileConfig);
    }

    private (bool, ProfileConfig) SetProjectId(ProfileConfig profileConfig)
    {
        var projectId = Prompt.Input<string>(
            "Enter new project id",
            defaultValue: profileConfig.ProjectId);

        if (!string.IsNullOrEmpty(projectId))
        {
            profileConfig.ProjectId = projectId;
            
            return (true, profileConfig);
        }

        return (false, profileConfig);
    }

    private (bool, ProfileConfig) ImportJsonConfigurationFromClipboard(ProfileConfig profileConfig)
    {
        var newJson = ClipboardService.GetText()?.Trim(); 
        
        if (!string.IsNullOrWhiteSpace(newJson))
        {
            try
            {
                var newProfileConfig = JsonSerializationHelper.Deserialize<ProfileConfig>(newJson);

                return new (newProfileConfig != null, newProfileConfig);
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteLineNotification(newJson);

                ConsoleHelper.WriteLineError(e.Message);

                return (false, profileConfig);
            }
        }

        return (false, profileConfig);
    }
    
    private (bool, ProfileConfig)  ExportJsonConfigurationIntoClipboard(ProfileConfig profileConfig)
    {
        var json = JsonSerializationHelper.Serialize(profileConfig);
        
        ClipboardService.SetText(json);

        ConsoleHelper.WriteLineNotification(json);

        return (false, profileConfig);
    }
    
    private (bool, ProfileConfig) SetDefaultConfiguration(ProfileConfig profileConfig)
    {
        var newProfileConfig = new ProfileConfig();
        
        // Don't reset project id
        newProfileConfig.ProjectId = profileConfig.ProjectId ?? newProfileConfig.ProjectId;
        
        return (true, newProfileConfig);
    }
}