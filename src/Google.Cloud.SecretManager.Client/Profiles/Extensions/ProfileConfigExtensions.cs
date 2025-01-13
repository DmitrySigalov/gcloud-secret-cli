using Google.Cloud.SecretManager.Client.Helpers;

namespace Google.Cloud.SecretManager.Client.Profiles.Extensions;

public static class ProfileConfigExtensions
{
    public static bool IsValid(this ProfileConfig profileConfig) =>
        profileConfig?.Filters?.Any() == true;

    public static ProfileConfig CloneObject(this ProfileConfig profileConfig)
    {
        var json = JsonSerializationHelper.Serialize(profileConfig);
        
        return JsonSerializationHelper.Deserialize<ProfileConfig>(json);
    }
}