using Grpc.Core;

namespace GCloud.Secret.Client.Profiles;

public class SecretDetails
{
    public string ConfigPath { get; set; }
    
    public string EnvironmentVariable { get; set; }

    public StatusCode AccessStatusCode { get; set; } = StatusCode.Unknown;

    public string DecodedValue { get; set; }
}