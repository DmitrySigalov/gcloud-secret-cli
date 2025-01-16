using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.V1;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.GCloud.Impl;

public class SecretManagerProviderImpl : ISecretManagerProvider
{
    public async Task PocAsync(CancellationToken cancellationToken)
    {
        var projectId = "lsports-poc";
        
        ConsoleHelper.WriteLineNotification($"Project ID: {projectId}");
        
        var tests = new Dictionary<string, Func<string, CancellationToken, Task>>
        {
            [$"{nameof(ListSecretsAsync_Paging)}"] = ListSecretsAsync_Paging,
            [$"{nameof(ListSecretsAsync_LazyPerformance)}"] = ListSecretsAsync_LazyPerformance,
        };
        
        var testName = Prompt.Select(
            "Select test",
            tests.Select(x => x.Key),
            defaultValue: tests.Keys.First());

        if (tests.TryGetValue(testName, out var test))
        {
            await test(projectId, cancellationToken);
        }
    }

    private async Task ListSecretsAsync_Paging(string projectId, CancellationToken cancellationToken)
    {
        // Create client
        var secretManagerServiceClient = await SecretManagerServiceClient.CreateAsync(cancellationToken);

        // Initialize request argument(s)
        ListSecretsRequest request = new ListSecretsRequest
        {
            ParentAsProjectName = ProjectName.FromProject(projectId),
            Filter = "",
        };
        
        // Make the request
        var response = secretManagerServiceClient.ListSecretsAsync(request);

        // Or iterate over pages (of server-defined size), performing one RPC per page
        await response.AsRawResponses().ForEachAsync((ListSecretsResponse page) =>
        {
            // Do something with each page of items
            Console.WriteLine("A page of results:");
            foreach (Secret item in page)
            {
                // Do something with each item
                Console.WriteLine(item.Name);
            }
        }, cancellationToken);    
    }

    private async Task ListSecretsAsync_LazyPerformance(string projectId, CancellationToken cancellationToken)
    {
        // Create client
        var secretManagerServiceClient = await SecretManagerServiceClient.CreateAsync(cancellationToken);

        // Initialize request argument(s)
        ListSecretsRequest request = new ListSecretsRequest
        {
            ParentAsProjectName = ProjectName.FromProject(projectId),
            Filter = "",
        };
        
        // Make the request
        var response = secretManagerServiceClient.ListSecretsAsync(request);

        // Iterate over all response items, lazily performing RPCs as required
        await response.ForEachAsync(
            Console.WriteLine, 
            cancellationToken);
    }
}