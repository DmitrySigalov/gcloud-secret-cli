namespace Google.Cloud.SecretManager.Client.VersionControl.Helpers;

public static class VersionControlConsts
{
    public const string FILE_NAME = "version-control.json";

    public static TimeSpan CheckInterval => TimeSpan.FromDays(1);
}