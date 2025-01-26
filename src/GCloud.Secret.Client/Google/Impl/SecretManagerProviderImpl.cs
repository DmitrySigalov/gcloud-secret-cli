using GCloud.Secret.Client.Profiles;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Grpc.Core;

namespace GCloud.Secret.Client.Google.Impl;

public class SecretManagerProviderImpl : ISecretManagerProvider
{
    private const int PAGE_SIZE = 10;

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

    public async Task ApplySecretLatestValueAsync(string projectId,
        string secretId,
        SecretDetails secretDetails,
        CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(cancellationToken);

        // Initialize request argument(s)
        var request = new AccessSecretVersionRequest
        {
            SecretVersionName = SecretVersionName.FromProjectSecretSecretVersion(
                projectId,
                secretId,
                "latest"),
        };

        try
        {
            // Make the request
            var response = await client.AccessSecretVersionAsync(request, cancellationToken);

            // Decode the secret payload
            var decodedValue = response.Payload?.Data?.ToStringUtf8();

            secretDetails.AccessStatusCode = StatusCode.OK;
            secretDetails.DecodedValue = decodedValue;
            
        }
        catch (RpcException e)
        {
            secretDetails.AccessStatusCode = e.StatusCode;
            secretDetails.DecodedValue = null;
        }
    }

    private Task<SecretManagerServiceClient> GetClientAsync(CancellationToken cancellationToken) =>
        SecretManagerServiceClient.CreateAsync(cancellationToken);
}