using System.Reflection;
using Google.Cloud.SecretManager.Client.Common;

namespace Google.Cloud.SecretManager.Client.VersionControl.Impl;

public class VersionControlImpl : IVersionControl
{
    public Task CheckVersionAsync(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"Current version is '{Assembly.GetEntryAssembly()?.GetName().Version}'");
        Console.WriteLine(Assembly.GetEntryAssembly()?.GetName().Name);
        
        return Task.CompletedTask;
    }
}