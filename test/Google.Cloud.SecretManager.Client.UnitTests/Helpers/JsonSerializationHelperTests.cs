using Xunit;

namespace Google.Cloud.SecretManager.Client.Helpers;

public class JsonSerializationHelperTests
{
    public class TestClass
    {
        public string StringProperty { get; set; }
    }

    [Fact]
    public void Serialize_Deserialize_Equals()
    {
        var beforeJson = new TestClass { StringProperty = "Test" };
        
        var json = JsonSerializationHelper.Serialize(beforeJson);
        Assert.NotNull(json);
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);

        var afterJson = JsonSerializationHelper.Deserialize<TestClass>(json);
        Assert.NotSame(beforeJson, afterJson);
        Assert.NotNull(afterJson);
        Assert.Equal(beforeJson.StringProperty, afterJson.StringProperty);
    }
}