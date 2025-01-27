using GCloud.Secret.Client.Profiles;

namespace GCloud.Secret.Client.Commands;

public class CommandState
{
    public required CancellationToken CancellationToken { get; init; } 
    
    public string ProfileName { get; set; }
    
    public ProfileConfig ProfileConfig { get; set; }
}