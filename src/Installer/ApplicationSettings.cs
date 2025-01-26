namespace Installer;

public static class ApplicationSettings
{
    public static string AppName => "gcloud-secret-cli";

    public static string Shortcut => "gscli";

    public static string ProjectPath => "src/GCloud.Secret.Client/GCloud.Secret.Client.csproj";

    public static Dictionary<string, Func<string>> DefaultArguments => 
        new()
        {
            ["BuildWorkingDirectory"] = () => "../",
            ["OsxAppPath"] = () => string.Format($"/opt/{AppName}"),
            ["OsxPathsD"] = () => string.Format($"/etc/paths.d/{AppName}"),
            ["OsxShortcut"] = () => Shortcut,
            ["WinAppPath"] = () =>
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                return Path.Combine(path, AppName);
            },
        };
}