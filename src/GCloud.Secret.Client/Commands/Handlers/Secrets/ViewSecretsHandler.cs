using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.EnvironmentVariables;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.EnvironmentVariables.Helpers;
using GCloud.Secret.Client.Profiles.Helpers;
using Sharprompt;

namespace GCloud.Secret.Client.Commands.Handlers.Secrets;

public class ViewSecretsHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;
    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

    public ViewSecretsHandler(
        IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider)
    {
        _profileConfigProvider = profileConfigProvider;
        _environmentVariablesProvider = environmentVariablesProvider;
    }
    
    public string CommandName => "view-secrets";
            
    public string ShortName => "vs";

    public string Description => "View profile secrets dump";

    public Task<ContinueStatusEnum> Handle(CommandState commandState)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var currentEnvironmentDescriptor = _environmentVariablesProvider.Get() ?? new EnvironmentDescriptor();

        if (!string.IsNullOrEmpty(currentEnvironmentDescriptor.ProfileName))
        {
            ConsoleHelper.WriteLineWarn($"Current active profile is [{currentEnvironmentDescriptor.ProfileName}] in the environment variables system");
        }

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

        if (commandState.ProfileConfig == null)
        {
            commandState.ProfileConfig = SpinnerHelper.Run(
                () => _profileConfigProvider.GetByName(commandState.ProfileName),
                $"Read profile [{commandState.ProfileName}]");

            if (commandState.ProfileConfig == null)
            {
                ConsoleHelper.WriteLineError($"Not found profile [{commandState.ProfileName}]");

                return Task.FromResult(ContinueStatusEnum.Exit);
            }

            commandState.ProfileConfig.PrintProfileConfig();
        }

        commandState.SecretsDump = _profileConfigProvider.ReadSecrets(commandState.ProfileName);

        if (commandState.SecretsDump == null)
        {
            ConsoleHelper.WriteLineNotification($"Not found dump with accessed secret values according to profile [{commandState.ProfileName}]");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        commandState.SecretsDump.PrintSecretsMappingIdNamesAccessValues();

        var newEnvironmentVariables = commandState.SecretsDump.ToEnvironmentDictionary();
        
        if (currentEnvironmentDescriptor.HasDiff(newEnvironmentVariables))
        {
            ConsoleHelper.WriteLineError(
                $"Profile [{commandState.ProfileName}] has different secret values with the environment variables system");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }
        
        ConsoleHelper.WriteLineInfo(
            $"Profile [{commandState.ProfileName}] is fully synchronized with the environment variables system");

        return Task.FromResult(ContinueStatusEnum.Exit);
    }
}