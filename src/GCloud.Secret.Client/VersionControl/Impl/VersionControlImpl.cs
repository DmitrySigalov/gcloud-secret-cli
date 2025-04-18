using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.GitHub;
using GCloud.Secret.Client.UserRuntime;
using GCloud.Secret.Client.VersionControl.Helpers;
using Microsoft.Extensions.Logging;

namespace GCloud.Secret.Client.VersionControl.Impl;

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
        Console.WriteLine($"Runtime version is '{VersionHelper.RuntimeVersion}'");
        
        var checkVersionInfo = await GetCheckVersionInfoAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(checkVersionInfo.LatestRelease?.Tag_Name))
        {
            ConsoleHelper.WriteWarn("Warning: ");
            Console.WriteLine("Missing information about latest release version");

            return;
        }

        if (!checkVersionInfo.LatestRelease.Tag_Name.Equals(VersionHelper.RuntimeVersion))
        {
            ConsoleHelper.WriteNotification("Warning: ");
            Console.Write($"New release version is available. ");
            Console.WriteLine($"To upgrade version to '{checkVersionInfo.LatestRelease.Tag_Name}', visit {checkVersionInfo.LatestRelease.Html_Url}");
        }
    }

    private async Task<CheckVersionInfo> GetCheckVersionInfoAsync(CancellationToken cancellationToken)
    {
        var currentCheckVersionDump = ReadReleaseVersionDump();

        if (ShouldQueryForNewVersion(currentCheckVersionDump) == false)
        {
            return currentCheckVersionDump;
        }
        
        var gitHubResponse = await _gitHubClient.GetLatestReleaseAsync(cancellationToken);

        var newCheckVersionDump = new CheckVersionInfo
        {
            LastCheckRuntimeReleaseVersion = VersionHelper.RuntimeVersion,
            LastCheckTime = DateTime.UtcNow,
            LatestRelease = gitHubResponse?.Data ?? 
                            currentCheckVersionDump?.LatestRelease, // In case of error don't delete last saved release version
        };

        SaveReleaseVersionDump(newCheckVersionDump);

        return newCheckVersionDump;
    }

    private bool ShouldQueryForNewVersion(CheckVersionInfo checkVersionInfo)
    {
        if (checkVersionInfo == null)
        {
            return true;
        }
        
        if (!VersionHelper.RuntimeVersion.Equals(checkVersionInfo.LastCheckRuntimeReleaseVersion, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        if (checkVersionInfo.LastCheckTime < DateTime.UtcNow.Subtract(VersionControlConsts.CheckInterval))
        {
            return true;
        }
        
        return false;
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
        var json = JsonSerializationHelper.Serialize(newDump);
        
        _userFilesProvider.WriteTextFile(VersionControlConsts.FILE_NAME,
            json,
            FolderTypeEnum.UserToolConfiguration);
    }
}