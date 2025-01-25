namespace Google.Cloud.SecretManager.Client.VersionControl.Helpers;

public static class VersionHelper
{
    public static string RuntimeVersion
    {
        get
        {
            var assemblyVersion = typeof(VersionHelper).Assembly.GetName().Version;
            
            return $"v{assemblyVersion!.Major}.{assemblyVersion!.Minor}.{assemblyVersion!.Build}";
        }
    } 
}