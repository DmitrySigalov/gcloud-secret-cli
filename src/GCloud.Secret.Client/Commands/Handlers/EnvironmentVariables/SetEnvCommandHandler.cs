using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.EnvironmentVariables;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.Profiles.Helpers;
using Sharprompt;

namespace GCloud.Secret.Client.Commands.Handlers.EnvironmentVariables;

public class SetEnvCommandHandler : ICommandHandler
{
    public const string COMMAND_NAME = "set-environment";

    private readonly IProfileConfigProvider _profileConfigProvider;
    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

    public SetEnvCommandHandler(
        IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider)
    {
        _profileConfigProvider = profileConfigProvider;
        _environmentVariablesProvider = environmentVariablesProvider;
    }
    
    public string CommandName => COMMAND_NAME;
        
    public string ShortName => "se";

    public string Description => "Set environment variables from secrets dump";
    
    public Task<ContinueStatusEnum> Handle(CommandState commandState)
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

                return Task.FromResult(ContinueStatusEnum.Exit);
            }

            commandState.ProfileName =
                profileNames.Count == 1
                    ? profileNames.Single()
                    : Prompt.Select(
                        "Select profile",
                        items: profileNames,
                        defaultValue: currentEnvironmentDescriptor.ProfileName);
        }

        if (commandState.SecretsDump == null)
        {
            commandState.SecretsDump = _profileConfigProvider.ReadSecrets(commandState.ProfileName);
            if (commandState.SecretsDump == null)
            {
                ConsoleHelper.WriteLineNotification($"Not found dump with secret values according to profile [{commandState.ProfileName}]");

                return Task.FromResult(ContinueStatusEnum.Exit);
            }
        }

        commandState.SecretsDump.PrintSecretsMappingIdNamesAccessValues();

        var newDescriptor = new EnvironmentDescriptor
        {
            ProfileName = commandState.ProfileName,
            Variables = commandState.SecretsDump.ToEnvironmentDictionary(),
        };
        
        if (!newDescriptor.Variables.Any())
        {
            ConsoleHelper.WriteLineNotification($"Not found any valid secret value in dump according to profile [{commandState.ProfileName}]");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        _environmentVariablesProvider.Set(newDescriptor,
            ConsoleHelper.WriteLineNotification);

        Console.WriteLine();
        ConsoleHelper.WriteLineInfo(
            $"DONE - Profile [{commandState.ProfileName}] ({newDescriptor.Variables.Count} secrets with value) has synchronized with the environment variables system");

        return Task.FromResult(ContinueStatusEnum.Exit);
    }
}