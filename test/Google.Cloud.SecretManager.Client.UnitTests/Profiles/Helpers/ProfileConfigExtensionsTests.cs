using Xunit;

namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public class ProfileConfigExtensionsTests
{
    public void CloneObject_Check()
    {
        var source = new ProfileConfig
        {
            ProjectId = "test-project",
            PathFilters = new()
            {
                "test1",
                "test2",
            },
        };

        var result = source.CloneObject();

        Assert.NotNull(result);
        
        Assert.Equal(source.ProjectId, result.ProjectId);
        Assert.Equal(source.PathFilters, result.PathFilters);
    }
}