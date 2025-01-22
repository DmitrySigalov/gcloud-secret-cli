using Grpc.Core;
using Newtonsoft.Json;

namespace Google.Cloud.SecretManager.Client.Profiles;

public class SecretDetails
{
    public string ConfigPath { get; set; }
    
    public string EnvironmentVariable { get; set; }
    
    [JsonIgnore]
    public RpcException AccessException { get; set; }
    
    public string DecodedValue { get; set; }
}