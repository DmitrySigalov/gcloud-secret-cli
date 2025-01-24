using System.Reflection;

namespace Google.Cloud.SecretManager.Client.VersionControl.Impl;

public class VersionControlImpl : IVersionControl
{
    public Task CheckVersionAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Current version is '{Assembly.GetEntryAssembly()?.GetName().Version}'");
        
        return Task.CompletedTask;
    }
}