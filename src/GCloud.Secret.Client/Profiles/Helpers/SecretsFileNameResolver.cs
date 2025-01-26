namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public static class SecretsFileNameResolver
{
    private const string SecretsFileExtension = ".secrets.json";

    public static string SearchFileNamePattern => $"*{SecretsFileExtension}";

    public static string BuildFileName(string profileName)
    {
        if (string.IsNullOrEmpty(profileName))
        {
            throw new ArgumentNullException(nameof(profileName));
        }

        return profileName + SecretsFileExtension;
    }
}