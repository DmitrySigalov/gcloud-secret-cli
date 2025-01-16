using Xunit;

namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public class ProfileConfigExtensionsTests
{
    [Fact]
    public void IsValid_True()
    {
        var source = new ProfileConfig
        {
            Filters = new HashSet<string>
            {
                "test",
            },
        };
        Assert.True(source.IsValid());
    }

    [Fact]
    public void IsValid_False()
    {
        Assert.False(ProfileConfigExtensions.IsValid(null));

        var source = new ProfileConfig();
        Assert.False(source.IsValid());
        
        source.Filters = null;
        Assert.False(source.IsValid());
        
        source.Filters = new HashSet<string>();
        Assert.False(source.IsValid());
    }

    public void CloneObject_Check()
    {
        var source = new ProfileConfig
        {
            EnvironmentVariablePrefix = "TEST_",
            RemoveEnvironmentVariableStartDelimiter = true,
            BaseDelimiter = '*',
            Filters = new HashSet<string>
            {
                "test1",
                "test2",
            },
        };

        var result = source.CloneObject();

        Assert.NotNull(result);
        
        Assert.Equal(source.EnvironmentVariablePrefix, result.EnvironmentVariablePrefix);
        Assert.Equal(source.RemoveEnvironmentVariableStartDelimiter, result.RemoveEnvironmentVariableStartDelimiter);
        Assert.Equal(source.BaseDelimiter, result.BaseDelimiter);
        Assert.Equal(source.Filters, result.Filters);
    }
}