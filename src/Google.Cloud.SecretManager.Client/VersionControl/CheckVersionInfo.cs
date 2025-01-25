using Google.Cloud.SecretManager.Client.GitHub;

namespace Google.Cloud.SecretManager.Client.VersionControl;

public class CheckVersionInfo
{
    public DateTime LastCheckTime { get; set; }
    
    public string LastCheckRuntimeReleaseVersion { get; set; }
    
    public GitHubModel.Release LatestRelease { get; set; }
}