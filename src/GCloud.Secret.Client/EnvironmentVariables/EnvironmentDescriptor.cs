namespace GCloud.Secret.Client.EnvironmentVariables;

public class EnvironmentDescriptor
{
    public string ProfileName { get; set; }
    
    public SortedDictionary<string, string> Variables { get; set; } = new(); 
}