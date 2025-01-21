using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.V1;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.GCloud.Impl;

public static class PocTests
{
    private class TestArgs
    {
        public required SecretManagerServiceClient Client { get; init; }
        public required string ProjectId { get; init; }
        public string Filter  { get; init; }
        public string SecretId { get; init; }
        public int PageSize { get; init; }
    }
    
    public static async Task RunAsync(CancellationToken cancellationToken)
    {
        var testArgs = new TestArgs
        {
            Client = await SecretManagerServiceClient.CreateAsync(cancellationToken),
            ProjectId = "lsports-poc",
            Filter = "",
            SecretId = "redis_lsports-poc_endpoint-missing",
            PageSize = 5,
        };
        
        var tests = new Dictionary<string, Func<TestArgs, CancellationToken, Task>>
        {
            [$"{nameof(ListSecretsAsync)}"] = ListSecretsAsync,
            [$"{nameof(GetSecretAsync)}"] = GetSecretAsync,
            [$"{nameof(GetSecretVersionAsync)}"] = GetSecretVersionAsync,
            [$"{nameof(AccessSecretVersionAsync)}"] = AccessSecretVersionAsync,
        };
        
        var testName = Prompt.Select(
            "Select test",
            tests.Select(x => x.Key),
            defaultValue: tests.Keys.First());

        if (tests.TryGetValue(testName, out var test))
        {
            ConsoleHelper.WriteLineInfo($"- {nameof(testArgs.ProjectId)}: {testArgs.ProjectId}");
        
            await test(testArgs, cancellationToken);
        }
    }

    private static async Task ListSecretsAsync(TestArgs testArgs, CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineInfo($"- {nameof(testArgs.Filter)}: {testArgs.Filter}");
        ConsoleHelper.WriteLineInfo($"- {nameof(testArgs.PageSize)}: {testArgs.PageSize}");

        // Initialize request argument(s)
        var request = new ListSecretsRequest
        {
            ParentAsProjectName = ProjectName.FromProject(testArgs.ProjectId),
            Filter = testArgs.Filter,
            PageSize = testArgs.PageSize,
        };
        
        // Make the request
        var response = testArgs.Client.ListSecretsAsync(request);

        var pageNumber = 0;

        // Or iterate over pages (of server-defined size), performing one RPC per page
        await response.AsRawResponses().ForEachAsync(page =>
        {
            // Do something with each page of items
            ConsoleHelper.WriteLineWarn($"Page #{++pageNumber}:");
            foreach (var item in page)
            {
                Console.WriteLine("- " + item);
            }
        }, cancellationToken);    
    }

    private static async Task GetSecretAsync(TestArgs testArgs, CancellationToken cancellationToken)
    {
        // Initialize request argument(s)
        var request = new GetSecretRequest
        {
            SecretName = SecretName.FromProjectSecret(testArgs.ProjectId, testArgs.SecretId),
        };
        
        // Make the request
        var response = await testArgs.Client.GetSecretAsync(request, cancellationToken);
        
        Console.WriteLine(response);
    }

    private static async Task GetSecretVersionAsync(TestArgs testArgs, CancellationToken cancellationToken)
    {
        // Initialize request argument(s)
        var request = new GetSecretVersionRequest
        {
            SecretVersionName = SecretVersionName.FromProjectSecretSecretVersion(
                testArgs.ProjectId, 
                testArgs.SecretId,
                "latest"),
        };
        
        // Make the request
        var response = await testArgs.Client.GetSecretVersionAsync(request, cancellationToken);
        
        Console.WriteLine(response);
    }

    private static async Task AccessSecretVersionAsync(TestArgs testArgs, CancellationToken cancellationToken)
    {
        // Initialize request argument(s)
        var request = new AccessSecretVersionRequest
        {
            SecretVersionName = SecretVersionName.FromProjectSecretSecretVersion(
                testArgs.ProjectId, 
                testArgs.SecretId,
                "latest"),
        };
        
        // Make the request
        var response = await testArgs.Client.AccessSecretVersionAsync(request, cancellationToken);
        
        // Decode the secret payload
        string payload = response.Payload.Data.ToStringUtf8();
        Console.WriteLine(payload);
    }
}