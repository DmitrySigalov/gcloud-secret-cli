using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.Commands.Handlers;

public class ViewProfileHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    public ViewProfileHandler(
        IProfileConfigProvider profileConfigProvider)
    {
        _profileConfigProvider = profileConfigProvider;
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

        var selectedProfileName = 
            profileNames.Count == 1
            ? profileNames.Single()
            : Prompt.Select(
                "Select profile",
                items: profileNames);

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
            ConsoleHelper.WriteLineNotification($"No dumped secrets according to profile [{selectedProfileName}]");
            
            return Task.CompletedTask;
        }

        currentSecrets.PrintSecretsMappingIdNamesAccessValues();

        return Task.CompletedTask;
    }
}