namespace Google.Cloud.SecretManager.Client.Profiles;

public sealed class ProfileConfig
{
    public string ProjectId { get; set; } = "<google-project-id>";

    public char SecretIdDelimiter { get; set; } = '_';

    public string EnvironmentVariablePrefix { get; set; }

    public char ConfigPathDelimiter { get; set; } = '_';

    public HashSet<string> PathFilters { get; set; } = new() { "" };
}