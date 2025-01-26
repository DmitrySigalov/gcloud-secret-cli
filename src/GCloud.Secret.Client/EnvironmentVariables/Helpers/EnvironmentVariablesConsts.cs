namespace GCloud.Secret.Client.EnvironmentVariables.Helpers;

public static class EnvironmentVariablesConsts
{
    private static string BaseVariableName => "GCLOUD_SECRETS_CLI"; 
    
    public static string GetClientToolVariableName(string name) => $"{BaseVariableName}_{name}".ToUpper(); 
    
    public static char VariableNameDelimiter => '_';
    
    public static char[] InvalidVariableNameCharacters => new []
    {
        '/', '\\', ':', '-', '.', ',', '\'', '"', '`', '{', '}', '[', ']', '$', ';', '(', ')', '@', '#', 
        '^', '?', '!', '&', ' ',
    };

    public static class FileNames
    {
        private static string Base => "environment-descriptor";

        public static string Descriptor => $"{Base}.json";

        public static string ScriptExtension => ".zshrc";

        public static string ScriptName => $"{Base}{ScriptExtension}";
    }
}