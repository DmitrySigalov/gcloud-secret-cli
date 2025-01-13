namespace Google.Cloud.SecretManager.Client.Profiles;

public sealed class ProfileConfig
{
    public string EnvironmentVariablePrefix { get; set; } = string.Empty;

    public bool RemoveEnvironmentVariableStartDelimiter { get; set; }

    public char BaseDelimiter { get; set; } = '_';

    public HashSet<string> Filters { get; set; } = new();
}