using GCloud.Secret.Client.Profiles;

namespace GCloud.Secret.Client.Commands;

public class CommandState
{
    public string ProfileName { get; set; }
    
    public ProfileConfig ProfileConfig { get; set; }
}