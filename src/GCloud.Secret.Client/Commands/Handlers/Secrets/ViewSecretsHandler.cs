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

    public string Description => "View profile secrets dump details";

    public Task<ContinueStatusEnum> Handle(CommandState commandState)
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

        var selectedProfileDo = SpinnerHelper.Run(
            () => _profileConfigProvider.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");

        if (selectedProfileDo == null)
        {
            ConsoleHelper.WriteLineError($"Not found profile [{selectedProfileName}]");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        selectedProfileDo.PrintProfileConfig();

        var currentSecrets = _profileConfigProvider.ReadSecrets(selectedProfileName);

        if (currentSecrets == null)
        {
            ConsoleHelper.WriteLineNotification($"Not found dump with secret values according to profile [{selectedProfileName}]");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }

        currentSecrets.PrintSecretsMappingIdNamesAccessValues();

        if (selectedProfileName != currentEnvironmentDescriptor.ProfileName)
        {
            ConsoleHelper.WriteLineNotification(
                $"Profile [{selectedProfileName}] is inactive in the environment variables system");
        }

        var newEnvironmentVariables = currentSecrets.ToEnvironmentDictionary();
        
        if (currentEnvironmentDescriptor.HasDiff(newEnvironmentVariables))
        {
            ConsoleHelper.WriteLineError(
                $"Profile [{selectedProfileName}] has different secret values with the environment variables system");

            return Task.FromResult(ContinueStatusEnum.Exit);
        }
        
        ConsoleHelper.WriteLineInfo(
            $"Profile [{selectedProfileName}] is fully synchronized with the environment variables system");

        return Task.FromResult(ContinueStatusEnum.Exit);
    }
}