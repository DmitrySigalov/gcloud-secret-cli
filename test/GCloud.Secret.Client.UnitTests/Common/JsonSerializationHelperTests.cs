using Xunit;

namespace GCloud.Secret.Client.Common;

public class JsonSerializationHelperTests
{
    public class TestClass
    {
        public required string StringProperty { get; set; }
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