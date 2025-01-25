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
        Console.WriteLine($"Current version is '{Assembly.GetEntryAssembly()?.GetName().Version}'");

        var gitHubRelease = await GetLatestReleaseResponseAsync(cancellationToken);

        if (gitHubRelease.Data == null)
        {
            ConsoleHelper.WriteLineError($"Unavailable information about Latest official release version");
            return;
        }

        Console.WriteLine($"Latest release version is '{gitHubRelease.Data.Tag_Name}'");
        Console.WriteLine($"{gitHubRelease.Data.Html_Url}");
    }

    private async Task<GitHubModel.Response<GitHubModel.Release>> GetLatestReleaseResponseAsync(CancellationToken cancellationToken)
    {
        var currentGitHubRelease = ReadReleaseVersionDump();

        // if (currentDump != null)
        // {
        //     return currentDump;
        // }
        
        var newGitHubRelease = await _gitHubClient.GetLatestReleaseAsync(cancellationToken);

        SaveReleaseVersionDump(newGitHubRelease);

        return newGitHubRelease;
    }

    private GitHubModel.Response<GitHubModel.Release> ReadReleaseVersionDump()
    {
        try
        {
            var json = _userFilesProvider.ReadTextFileIfExist(VersionControlConsts.FILE_NAME, 
                FolderTypeEnum.UserToolConfiguration);

            if (!string.IsNullOrWhiteSpace(json))
            {
                return JsonSerializationHelper.Deserialize<GitHubModel.Response<GitHubModel.Release>>(json);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to read latest release version dump");
        }

        return null;
    }

    private void SaveReleaseVersionDump(GitHubModel.Response<GitHubModel.Release> newDump)
    {
        var json = JsonSerializationHelper.Serialize(newDump);
        
        _userFilesProvider.WriteTextFile(VersionControlConsts.FILE_NAME,
            json,
            FolderTypeEnum.UserToolConfiguration);
    }
}