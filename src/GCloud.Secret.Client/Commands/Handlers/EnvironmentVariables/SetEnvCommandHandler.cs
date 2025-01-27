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
    
    public Task<ContinueStatusEnum> Handle(CommandState commandState, CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleHelper.WriteLineError("Not found any profile");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        var currentEnvironmentDescriptor = _environmentVariablesProvider.Get() ?? new EnvironmentDescriptor();

        var selectedProfileName =
            profileNames.Count == 1
                ? profileNames.Single()
                : Prompt.Select(
                    "Select profile",
                    items: profileNames,
                    defaultValue: currentEnvironmentDescriptor.ProfileName);

        var newSecrets = _profileConfigProvider.ReadSecrets(selectedProfileName);
        if (newSecrets == null)
        {
            ConsoleHelper.WriteLineNotification($"Not found dump with secret values according to profile [{selectedProfileName}]");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        newSecrets.PrintSecretsMappingIdNamesAccessValues();

        var newDescriptor = new EnvironmentDescriptor
        {
            ProfileName = selectedProfileName,
            Variables = newSecrets.ToEnvironmentDictionary(),
        };
        
        if (!newDescriptor.Variables.Any())
        {
            ConsoleHelper.WriteLineNotification($"Not found any valid secret value in dump according to profile [{selectedProfileName}]");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        _environmentVariablesProvider.Set(newDescriptor,
            ConsoleHelper.WriteLineNotification);

        Console.WriteLine();
        ConsoleHelper.WriteLineInfo(
            $"DONE - Profile [{selectedProfileName}] ({newDescriptor.Variables.Count} secrets with value) has synchronized with the environment variables system");

        return Task.FromResult(ContinueStatusEnum.Exit);
    }
}