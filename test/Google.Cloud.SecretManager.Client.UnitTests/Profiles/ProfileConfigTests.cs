using Google.Cloud.SecretManager.Client.Helpers;
using Xunit;

namespace Google.Cloud.SecretManager.Client.Profiles;

public class ProfileConfigTests
{
    [Fact]
    public void Constr_Defaults()
    {
        var config = new ProfileConfig();
        
        Assert.Equal(String.Empty, config.EnvironmentVariablePrefix);
        Assert.False(config.RemoveEnvironmentVariableStartDelimiter);
        Assert.Equal('_', config.BaseDelimiter);
        Assert.Empty(config.Filters);
    }
    
    [Fact]
    public void Deserialize_FromEmptyJson_Defaults()
    {
        var json = "{}";
        
        var config = JsonSerializationHelper.Deserialize<ProfileConfig>(json);
        
        Assert.Equal(String.Empty, config.EnvironmentVariablePrefix);
        Assert.False(config.RemoveEnvironmentVariableStartDelimiter);
        Assert.Equal('_', config.BaseDelimiter);
        Assert.Empty(config.Filters);
    }
}