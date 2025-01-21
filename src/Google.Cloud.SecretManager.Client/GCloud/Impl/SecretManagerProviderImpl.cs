using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;

namespace Google.Cloud.SecretManager.Client.GCloud.Impl;

public class SecretManagerProviderImpl : ISecretManagerProvider
{
    private const int PAGE_SIZE = 3;
    
    public async Task<HashSet<string>> GetSecretIdsAsync(string projectId,
        CancellationToken cancellationToken = default)
    {
        var result = new List<string>();
        
        var client = await GetClientAsync(cancellationToken);
        
        var request = new ListSecretsRequest
        {
            ParentAsProjectName = ProjectName.FromProject(projectId),
            PageSize = PAGE_SIZE,
        };
        
        // Make the request
        var response = client.ListSecretsAsync(request);

        // Or iterate over pages (of server-defined size), performing one RPC per page
        await response.AsRawResponses().ForEachAsync(page =>
        {
            // Do something with each page of items
            foreach (var item in page)
            {
                result.Add(item.SecretName.SecretId);
            }
        }, cancellationToken);

        return result
            .Order()
            .ToHashSet();
    }
    
    private Task<SecretManagerServiceClient> GetClientAsync(CancellationToken cancellationToken) =>
        SecretManagerServiceClient.CreateAsync(cancellationToken);
}