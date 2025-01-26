using GCloud.Secret.Client.GitHub;

namespace GCloud.Secret.Client.VersionControl;

public class CheckVersionInfo
{
    public DateTime LastCheckTime { get; set; }
    
    public string LastCheckRuntimeReleaseVersion { get; set; }
    
    public GitHubModel.Release LatestRelease { get; set; }
}