using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.EnvironmentVariables;
using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers.Environment;

public class SetEnvCommandHandler : ICommandHandler
{
    public const string COMMAND_NAME = "set-env";

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
    
    public string Description => "Set environment variables";
    
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

        var newSecrets = _profileConfigProvider.ReadSecrets(selectedProfileName);
        if (newSecrets == null)
        {
            ConsoleHelper.WriteLineNotification($"Not dumped secret values according to profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }

        newSecrets.PrintSecretsMappingIdNamesAccessValues();

        var newDescriptor = new EnvironmentDescriptor
        {
            ProfileName = selectedProfileName,
            Variables = newSecrets.ToEnvironmentDictionary(),
        };
        var forceUpdate = false;
        
        if (!newDescriptor.Variables.Any())
        {
            ConsoleHelper.WriteLineNotification($"Not found any dumped secret values according to profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }
        
        if (!string.IsNullOrEmpty(currentEnvironmentDescriptor.ProfileName) &&
            !selectedProfileName.Equals(currentEnvironmentDescriptor.ProfileName, StringComparison.InvariantCultureIgnoreCase))
        {
            ConsoleHelper.WriteLineNotification(
                $"Start switch profile from [{currentEnvironmentDescriptor.ProfileName}] to [{selectedProfileName}] in the environment variables system");
        }

        if (!newDescriptor.HasDiff(currentEnvironmentDescriptor.Variables))
        {
            ConsoleHelper.WriteLineInfo(
                $"Profile [{selectedProfileName}] already is fully synchronized with the environment variables system");

            forceUpdate = Prompt.Select(
                "Force to update all environment variables",
                new[] { true, false, },
                defaultValue: false);

            if (!forceUpdate)
            {
                return Task.CompletedTask;
            }
        }

        _environmentVariablesProvider.Set(newDescriptor,
            skipCheckChanges: forceUpdate,
            ConsoleHelper.WriteLineNotification);

        Console.WriteLine();
        ConsoleHelper.WriteLineInfo(
            $"DONE - Profile [{selectedProfileName}] ({newDescriptor.Variables.Count} secrets with value) has synchronized with the environment variables system");

        return Task.CompletedTask;
    }
}