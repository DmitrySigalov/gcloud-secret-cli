using ConsoleTables;
using Google.Cloud.SecretManager.Client.Common;
using Grpc.Core;

namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public static class SecretDetailsExtensions
{
    public static void RemoveSecretsWithNoValue(this IDictionary<string, SecretDetails> secrets)
    {
        var notRetrievedSecretsIds = secrets
            .Where(x => x.Value.AccessException != null)
            .Select(x => x.Key)
            .ToHashSet();

        foreach (var notRetrievedSecretId in notRetrievedSecretsIds)
        {
            secrets.Remove(notRetrievedSecretId);
        }
    }
    
    public static void PrintSecretsMappingIdNames(this IDictionary<string, SecretDetails> secrets)
    {
        var table = new ConsoleTable("secret-id", "config-path", "environment-variable");

        foreach (var names in secrets)
        {
            table.AddRow(names.Key, 
                names.Value.ConfigPath, 
                names.Value.EnvironmentVariable);
        }

        table.Write(Format.Minimal);
    }
    
    public static async Task<int> PrintProgressGetSecretLatestDiffValuesAsync(this IDictionary<string, SecretDetails> secrets,
        Func<KeyValuePair<string, SecretDetails>, Task> secretValueGetter,
        IDictionary<string, SecretDetails> oldSecrets,
        CancellationToken cancellationToken)
    {
        var changesCounter = 0;
        
        ConsoleHelper.WriteLineInfo("Access to latest secret values...");

        var table = new ConsoleTable("sync-status", "secret-id", "decoded value/error");
        table.Write(Format.Minimal);

        foreach (var secretDetails in secrets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await secretValueGetter(secretDetails);
            
            var hasChanges = oldSecrets == null || 
                             !oldSecrets.TryGetValue(secretDetails.Key, out var oldSecret) ||
                             !secretDetails.Value.DecodedValue.Equals(oldSecret.DecodedValue);
            if (secretDetails.Value.AccessException != null)
            {
                hasChanges = oldSecrets?.ContainsKey(secretDetails.Key) == true;
            }

            Action<string> writeAction = ConsoleHelper.WriteInfo;
            var syncStatus = "PASS";

            if (secretDetails.Value.AccessException != null)
            {
                writeAction = ConsoleHelper.WriteError;
                syncStatus = secretDetails.Value.AccessException.StatusCode.ToString();
            }
            else if (hasChanges)
            {
                writeAction = ConsoleHelper.WriteWarn;
                syncStatus = "CHANGED";
                if (oldSecrets?.ContainsKey(secretDetails.Key) != true)
                {
                    syncStatus = "NEW";
                }
            }
            writeAction($"{syncStatus}\t");

            ConsoleHelper.WriteNotification($"{secretDetails.Key}\t");
            
            if (secretDetails.Value.AccessException == null)
            {
                Console.Write($"{secretDetails.Value.DecodedValue}");
            }
            else
            {
                ConsoleHelper.WriteError($"{secretDetails.Value.AccessException.Status.Detail}");
            }
            
            Console.WriteLine();

            if (hasChanges)
            {
                ++changesCounter;
            }
        }

        foreach (var oldSecretDetails in oldSecrets ?? new Dictionary<string, SecretDetails>())
        {
            if (!secrets.TryGetValue(oldSecretDetails.Key, out var newSecretDetails) ||
                newSecretDetails.AccessException != null)
            {
                ConsoleHelper.WriteWarn("DELETED\t");
                ConsoleHelper.WriteNotification($"{oldSecretDetails.Key}\t");
                Console.Write($"{oldSecretDetails.Value.DecodedValue}");
                ++changesCounter;
            }
        }
        
        Console.WriteLine();

        return changesCounter;
    }

    public static void PrintSecretsMappingIdNamesAccessValues(this IDictionary<string, SecretDetails> secrets)
    {
        var table = new ConsoleTable("secret-id", "config-path", "environment-variable", "decoded-value");

        foreach (var secretDetails in secrets)
        {
            table.AddRow(secretDetails.Key, 
                secretDetails.Value.ConfigPath, 
                secretDetails.Value.EnvironmentVariable, 
                secretDetails.Value.DecodedValue);
        }

        table.Write(Format.Minimal);
    }
}