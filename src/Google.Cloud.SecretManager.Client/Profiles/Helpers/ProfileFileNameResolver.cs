namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public static class ProfileFileNameResolver
{
    private const string ProfileFileExtension = ".profile.json";

    public static string SearchFileNamePattern => $"*{ProfileFileExtension}";

    public static string ExtractProfileName(string fileName)
    {
        if (fileName?.EndsWith(ProfileFileExtension) != true)
        {
            throw new ArgumentException("Invalid file name", fileName);
        }

        return fileName.Substring(0, fileName.Length - ProfileFileExtension.Length);
    }

    public static string BuildFileName(string profileName)
    {
        if (string.IsNullOrEmpty(profileName))
        {
            throw new ArgumentNullException(nameof(profileName));
        }

        return profileName + ProfileFileExtension;
    }
}