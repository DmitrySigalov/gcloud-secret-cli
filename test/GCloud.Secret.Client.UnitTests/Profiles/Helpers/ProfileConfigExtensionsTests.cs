using Xunit;

namespace GCloud.Secret.Client.Profiles.Helpers;

public class ProfileConfigExtensionsTests
{
    [Fact]
    public void BuildSecretDetails_WithDefaultProfileConfig_Check_EnvironmentVariables()
    {
        var profileConfig = new ProfileConfig
        {
            ProjectId = "test-project",
        };

        var secretIds = new HashSet<string>
        {
            "_folder1-with-prefix_sub-folder1_param1",
            "folder1-no-prefix_sub-folder1_param2",
            "folder2-no-prefix_sub-folder1_param1",
        };

        var expectedEnvironmentVariablesMapping = secretIds
            .ToDictionary(
                x => x,
                y => y.TrimStart('_').Replace('-', '_').ToUpper());

        var result = profileConfig.BuildSecretDetails(secretIds);
        
        Assert.NotNull(result);

        Assert.Equal(secretIds.Count, result.Count);

        Assert.All(result, check =>
        {
            Assert.Contains(check.Key, secretIds);
            
            Assert.Equal(expectedEnvironmentVariablesMapping[check.Key], check.Value.EnvironmentVariable);
        });
    }
    
    [Fact]
    public void BuildSecretDetails_WithDefaultProfileConfig_Check_ConfigPath()
    {
        var profileConfig = new ProfileConfig
        {
            ProjectId = "test-project",
        };

        var secretIds = new HashSet<string>
        {
            "_folder1-with-prefix_sub-folder1_param1",
            "folder1-no-prefix_sub-folder1_param2",
            "folder2-no-prefix_sub-folder1_param1",
        };

        var expectedConfigPathMapping = secretIds
            .ToDictionary(
                x => x,
                y => y.Replace('_', '/'));

        var result = profileConfig.BuildSecretDetails(secretIds);
        
        Assert.NotNull(result);

        Assert.Equal(secretIds.Count, result.Count);

        Assert.All(result, check =>
        {
            Assert.Contains(check.Key, secretIds);
            
            Assert.Equal(expectedConfigPathMapping[check.Key], check.Value.ConfigPath);
        });
    }
}