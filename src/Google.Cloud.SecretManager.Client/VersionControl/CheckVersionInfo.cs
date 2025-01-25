using Google.Cloud.SecretManager.Client.GitHub;

namespace Google.Cloud.SecretManager.Client.VersionControl;

public class CheckVersionInfo
{
    public DateTime LastRequestTime { get; set; } = DateTime.UtcNow;
    
    public Version LastCheckVersion { get; set; }
    
    public GitHubModel.Release LatestRelease { get; set; }
}