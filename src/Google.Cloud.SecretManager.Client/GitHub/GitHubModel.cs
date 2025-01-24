namespace Google.Cloud.SecretManager.Client.GitHub;

public class GitHubModel
{
    public class Release
    {
        public string Name { get; set; }
        
        public string Tag_Name { get; set; }
    }
}