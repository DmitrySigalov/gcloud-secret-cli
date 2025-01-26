namespace Installer;

public static class ApplicationSettings
{
    public static string AppName => "gcloud-secret-manager-cli";

    public static string Shortcut => "gscli";

    public static string ProjectPath => "src/Google.Cloud.SecretManager.Client/Google.Cloud.SecretManager.Client.csproj";

    public static Dictionary<string, Func<string>> DefaultArguments => 
        new()
        {
            ["BuildWorkingDirectory"] = () => "../",
            ["OsxAppPath"] = () => string.Format($"/opt/{AppName}"),
            ["OsxPathsD"] = () => string.Format($"/etc/paths.d/{AppName}"),
            ["OsxShortcut"] = () => Shortcut,
        };
}