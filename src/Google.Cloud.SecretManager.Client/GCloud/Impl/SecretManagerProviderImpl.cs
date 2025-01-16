using Google.Cloud.SecretManager.Client.Common;

namespace Google.Cloud.SecretManager.Client.GCloud.Impl;

public class SecretManagerProviderImpl : ISecretManagerProvider
{
    public Task PocAsync(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineWarn("Not implemented yet!");
        
        return Task.CompletedTask;
    }
}