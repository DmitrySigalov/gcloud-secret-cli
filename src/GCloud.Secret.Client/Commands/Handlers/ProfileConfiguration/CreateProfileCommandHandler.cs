using System.ComponentModel.DataAnnotations;
using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.Profiles.Helpers;
using Sharprompt;
using IProfileConfigProvider = GCloud.Secret.Client.Profiles.IProfileConfigProvider;

namespace GCloud.Secret.Client.Commands.Handlers.ProfileConfiguration;

public class CreateProfileCommandHandler : ICommandHandler
{
    public const string COMMAND_NAME = "create-profile";
    
    private readonly IProfileConfigProvider _profileConfigProvider;

    public CreateProfileCommandHandler(IProfileConfigProvider profileConfigProvider)
    {
        _profileConfigProvider = profileConfigProvider;
    }

    public string CommandName => COMMAND_NAME;
        
    public string ShortName => "cp";

    public string Description => "Create profile configuration";
    
    public async Task<ContinueStatusEnum> Handle(CommandState commandState)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        if (string.IsNullOrWhiteSpace(commandState.ProfileName))
        {
            commandState.ProfileName = Prompt.Input<string>(
                "Enter new profile name (project id) ",
                defaultValue: "google-project-id",
                validators: new List<Func<object, ValidationResult>>
                {
                    check => ProfileNameValidationRule.Handle((string) check, profileNames),
                }).Trim();
        }
        else
        {
            var validationResult = ProfileNameValidationRule.Handle(commandState.ProfileName, profileNames);

            if (validationResult != ValidationResult.Success)
            {
                ConsoleHelper.WriteError(validationResult.ErrorMessage);

                return ContinueStatusEnum.Exit;
            }
        }
        
        commandState.ProfileConfig = new()
        {
            ProjectId = commandState.ProfileName,
        };
            
        SpinnerHelper.Run(
            () => _profileConfigProvider.Save(commandState.ProfileName, commandState.ProfileConfig),
            $"Save new profile [{commandState.ProfileName}] configuration with default settings");

        commandState.ProfileConfig.PrintProfileConfig();

        ConsoleHelper.WriteLineInfo($"DONE - Created profile [{commandState.ProfileName}]");
        Console.WriteLine();

        return ContinueStatusEnum.ConfigProfile;
    }
}