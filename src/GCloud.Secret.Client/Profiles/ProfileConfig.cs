namespace GCloud.Secret.Client.Profiles;

public sealed class ProfileConfig
{
    public string ProjectId { get; set; } = "<google-project-id>";

    public char SecretIdDelimiter { get; set; } = '_';

    public string EnvironmentVariablePrefix { get; set; }

    public bool RemoveStartDelimiter { get; set; } = true;

    public char SecretPathDelimiter { get; set; } = '/';
}