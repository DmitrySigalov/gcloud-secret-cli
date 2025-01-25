using System.Reflection;
using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.GitHub;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Google.Cloud.SecretManager.Client.VersionControl.Helpers;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.VersionControl.Impl;

public class VersionControlImpl : IVersionControl
{
    private readonly IGitHubClient _gitHubClient;
    private readonly IUserFilesProvider _userFilesProvider;
    private readonly ILogger _logger;

    public VersionControlImpl(IGitHubClient gitHubClient,
        IUserFilesProvider userFilesProvider,
        ILogger<VersionControlImpl> logger)
    {
        _gitHubClient = gitHubClient;
        _userFilesProvider = userFilesProvider;
        _logger = logger;
    }

    public async Task CheckVersionAsync(CancellationToken cancellationToken)
    {
        var checkVersionInfo = await GetCheckVersionInfoAsync(cancellationToken);

        Console.WriteLine($"Current version is '{checkVersionInfo.LastCheckVersion}'");

        if (checkVersionInfo.LatestRelease == null)
        {
            ConsoleHelper.WriteLineError($"Unavailable information about latest official release version");

            return;
        }

        Console.WriteLine($"Latest release version is '{checkVersionInfo.LatestRelease.Tag_Name}'");
        Console.WriteLine($"{checkVersionInfo.LatestRelease.Html_Url}");
    }

    private async Task<CheckVersionInfo> GetCheckVersionInfoAsync(CancellationToken cancellationToken)
    {
        var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
        
        var currentCheckVersionDump = ReadReleaseVersionDump();

        if (currentCheckVersionDump != null &&
            currentCheckVersionDump.LastCheckVersion == currentVersion)
        {
            return currentCheckVersionDump;
        }

        var gitHubResponse = await _gitHubClient.GetLatestReleaseAsync(cancellationToken);

        var newCheckVersionDump = new CheckVersionInfo
        {
            LastCheckVersion = currentVersion,
            LatestRelease = gitHubResponse?.Data,
        };

        SaveReleaseVersionDump(newCheckVersionDump);

        return newCheckVersionDump;
    }

    private CheckVersionInfo ReadReleaseVersionDump()
    {
        try
        {
            var json = _userFilesProvider.ReadTextFileIfExist(VersionControlConsts.FILE_NAME, 
                FolderTypeEnum.UserToolConfiguration);

            if (!string.IsNullOrWhiteSpace(json))
            {
                return JsonSerializationHelper.Deserialize<CheckVersionInfo>(json);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to read latest release version dump");
        }

        return null;
    }

    private void SaveReleaseVersionDump(CheckVersionInfo newDump)
    {
        newDump.LastRequestTime = DateTime.UtcNow;
        
        var json = JsonSerializationHelper.Serialize(newDump);
        
        _userFilesProvider.WriteTextFile(VersionControlConsts.FILE_NAME,
            json,
            FolderTypeEnum.UserToolConfiguration);
    }
}