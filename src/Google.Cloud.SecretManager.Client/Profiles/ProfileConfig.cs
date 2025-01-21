namespace Google.Cloud.SecretManager.Client.Profiles;

public sealed class ProfileConfig
{
    public string ProjectId { get; set; } = "<google-project-id>";

    public char SecretDelimiter { get; set; } = '_';

    public char PathDelimiter { get; set; } = '_'; // '/', '_'

    public HashSet<string> PathFilters { get; set; } = new() { "" };
    
    public EnvironmentVariableNameConfig EnvironmentVariables { get; set; } = new();

    public class EnvironmentVariableNameConfig
    {
        public bool RemoveStartDelimiter { get; set; }

        public string AddPrefix { get; set; }
    }
}