using GCloud.Secret.Client.Common;
using Xunit;

namespace GCloud.Secret.Client.Profiles;

public class ProfileConfigTests
{
    private void AssertDefaults(ProfileConfig profileConfig)
    {
        Assert.Equal("<google-project-id>", profileConfig.ProjectId);

        Assert.Equal('_', profileConfig.SecretIdDelimiter);
        Assert.Equal('/', profileConfig.ConfigPathDelimiter);
        
        Assert.Null(profileConfig.EnvironmentVariablePrefix);
        
        Assert.True(profileConfig.RemoveStartDelimiter);
    }
    
    [Fact]
    public void Constr_Defaults()
    {
        var profileConfig = new ProfileConfig();
        
        AssertDefaults(profileConfig);
    }
    
    [Fact]
    public void Deserialize_FromEmptyJson_Defaults()
    {
        var json = "{}";
        
        var profileConfig = JsonSerializationHelper.Deserialize<ProfileConfig>(json);
        
        AssertDefaults(profileConfig);
    }
}