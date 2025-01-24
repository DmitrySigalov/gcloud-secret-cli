using System.Reflection;
using Google.Cloud.SecretManager.Client.GitHub;

namespace Google.Cloud.SecretManager.Client.VersionControl.Impl;

public class VersionControlImpl : IVersionControl
{
    private readonly IGitHubClient _gitHubClient;

    public VersionControlImpl(IGitHubClient gitHubClient)
    {
        _gitHubClient = gitHubClient;
    }
    
    public async Task CheckVersionAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Current version is '{Assembly.GetEntryAssembly()?.GetName().Version}'");

        var gitHubRelease = await _gitHubClient.GetLatestReleaseAsync(cancellationToken);
        
        Console.WriteLine($"Latest release is '{gitHubRelease?.Name}' with tag '{gitHubRelease?.Tag_Name}'");
    }
}