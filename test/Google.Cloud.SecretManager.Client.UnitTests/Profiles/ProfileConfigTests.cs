// using Google.Cloud.SecretManager.Client.Common;
// using Xunit;
//
// namespace Google.Cloud.SecretManager.Client.Profiles;
//
// public class ProfileConfigTests
// {
//     [Fact]
//     public void Constr_Defaults()
//     {
//         var config = new ProfileConfig();
//         
//         Assert.Equal(String.Empty, config.EnvironmentVariablePrefix);
//         Assert.False(config.RemoveEnvironmentVariableStartDelimiter);
//         Assert.Equal('_', config.EnvironmentDelimiter);
//         Assert.Empty(config.PathFilters);
//     }
//     
//     [Fact]
//     public void Deserialize_FromEmptyJson_Defaults()
//     {
//         var json = "{}";
//         
//         var config = JsonSerializationHelper.Deserialize<ProfileConfig>(json);
//         
//         Assert.Equal(String.Empty, config.EnvironmentVariablePrefix);
//         Assert.False(config.RemoveEnvironmentVariableStartDelimiter);
//         Assert.Equal('_', config.EnvironmentDelimiter);
//         Assert.Empty(config.PathFilters);
//     }
// }