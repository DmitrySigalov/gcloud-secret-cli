using Google.Cloud.SecretManager.Client.Common;
using Xunit;

namespace Google.Cloud.SecretManager.Client.Profiles;

public class ProfileConfigTests
{
    private void AssertDefaults(ProfileConfig profileConfig)
    {
        Assert.Equal("<google-project-id>", profileConfig.ProjectId);

        Assert.Equal('_', profileConfig.SecretIdDelimiter);
        Assert.Equal('/', profileConfig.SecretPathDelimiter);
        
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