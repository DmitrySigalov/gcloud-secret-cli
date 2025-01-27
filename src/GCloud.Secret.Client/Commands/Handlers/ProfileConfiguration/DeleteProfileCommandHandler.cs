using GCloud.Secret.Client.Common;
using Sharprompt;
using IProfileConfigProvider = GCloud.Secret.Client.Profiles.IProfileConfigProvider;

namespace GCloud.Secret.Client.Commands.Handlers.ProfileConfiguration;

public class DeleteProfileCommandHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    public DeleteProfileCommandHandler(IProfileConfigProvider profileConfigProvider)
    {
        _profileConfigProvider = profileConfigProvider;
    }

    public string CommandName => "delete-profile";
    
    public string Description => "Delete profile configuration";
    
    public async Task<ContinueStatusEnum> Handle(CommandState commandState, CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(commandState.ProfileName))
        {
            var profileNames = SpinnerHelper.Run(
                _profileConfigProvider.GetNames,
                "Get profile names");

            commandState.ProfileName = Prompt.Select(
                "Select profile",
                items: profileNames);
        }

        if (commandState.ProfileConfig == null)
        {
            commandState.ProfileConfig = SpinnerHelper.Run(
                () => _profileConfigProvider.GetByName(commandState.ProfileName),
                $"Read selected profile [{commandState.ProfileName}]");
        }

        if (commandState.ProfileConfig == null)
        {
            ConsoleHelper.WriteLineError($"Not found profile [{commandState.ProfileName}]");

            return ContinueStatusEnum.Exit;
        }

        SpinnerHelper.Run(
            () => _profileConfigProvider.Delete(commandState.ProfileName),
            $"Delete profile [{commandState.ProfileName}]");
            
        ConsoleHelper.WriteLineInfo($"DONE - Deleted profile [{commandState.ProfileName}]");

        return ContinueStatusEnum.Exit;
   }
}