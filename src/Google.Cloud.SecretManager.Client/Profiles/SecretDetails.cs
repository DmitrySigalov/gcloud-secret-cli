using Newtonsoft.Json;

namespace Google.Cloud.SecretManager.Client.Profiles;

public class SecretDetails
{
    public string EnvironmentVariable { get; set; }
    
    public string ConfigPath { get; set; }
    
    [JsonIgnore]
    public Exception AccessException { get; set; }
    
    public string DecodedValue { get; set; }
}