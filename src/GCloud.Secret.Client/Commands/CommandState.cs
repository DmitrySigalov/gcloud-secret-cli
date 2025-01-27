using GCloud.Secret.Client.Profiles;

namespace GCloud.Secret.Client.Commands;

public class CommandState
{
    private string _profileName;
    private ProfileConfig _profileConfig;
    
    public required CancellationToken CancellationToken { get; init; }

    public string ProfileName
    {
        get => _profileName;
        set
        {
            _profileName = value;
            ProfileConfig = null;
        }
    }

    public ProfileConfig ProfileConfig
    {
        get => _profileConfig;
        set
        {
            _profileConfig = value;
            SecretsDump = null;
        }
    }

    public IDictionary<string, SecretDetails> SecretsDump { get; set; }
}