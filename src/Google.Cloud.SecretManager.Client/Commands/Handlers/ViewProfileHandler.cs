using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.EnvironmentVariables;
using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class ViewProfileHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;
    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

    public ViewProfileHandler(
        IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider)
    {
        _profileConfigProvider = profileConfigProvider;
        _environmentVariablesProvider = environmentVariablesProvider;
    }
    
    public string CommandName => "view";
    
    public string Description => "View profile configuration and dumped secrets";

    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleHelper.WriteLineError("No found any profile");

            return Task.CompletedTask;
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
            ConsoleHelper.WriteLineError($"No found profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }

        selectedProfileDo.PrintProfileConfig();

        var currentSecrets = _profileConfigProvider.ReadSecrets(selectedProfileName);

        if (currentSecrets == null)
        {
            ConsoleHelper.WriteLineNotification($"No secret values dump according to profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }

        currentSecrets.PrintSecretsMappingIdNamesAccessValues();

        if (selectedProfileName != currentEnvironmentDescriptor.ProfileName)
        {
            ConsoleHelper.WriteLineError(
                $"Profile [{selectedProfileName}] is inactive in the environment variables system");
        }

        var newEnvironmentVariables = currentSecrets.ToEnvironmentDictionary();
        
        if (currentEnvironmentDescriptor.HasDiff(newEnvironmentVariables))
        {
            ConsoleHelper.WriteLineError(
                $"Profile [{selectedProfileName}] has different secret values with the environment variables system");

            return Task.CompletedTask;
        }
        
        ConsoleHelper.WriteLineInfo(
            $"Profile [{selectedProfileName}] is fully synchronized with the environment variables system");

        return Task.CompletedTask;
    }
}