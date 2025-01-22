using ConsoleTables;
using Google.Cloud.SecretManager.Client.Common;

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
    
    public static async Task<bool> PrintProgressCompareGetSecretLatestValuesAsync(this IDictionary<string, SecretDetails> secrets,
        Func<KeyValuePair<string, SecretDetails>, Task> secretValueGetter,
        IDictionary<string, SecretDetails> oldSecrets,
        CancellationToken cancellationToken)
    {
        var compareResult = false;
        
        ConsoleHelper.WriteLineInfo("Access to latest secret values...");

        foreach (var secretDetails in secrets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await secretValueGetter(secretDetails);
            
            var hasChanged = oldSecrets == null || 
                             !oldSecrets.TryGetValue(secretDetails.Key, out var oldSecret) ||
                             !oldSecret.Equals(secretDetails.Value);
            if (secretDetails.Value.AccessException != null)
            {
                hasChanged = oldSecrets?.ContainsKey(secretDetails.Key) == true;
            }

            if (secretDetails.Value.AccessException != null)
            {
                ConsoleHelper.WriteError($"{secretDetails.Value.AccessException.StatusCode}\t");
            }
            else if (hasChanged)
            {
                var changeStatus = "CHANGED\t";
                if (oldSecrets?.ContainsKey(secretDetails.Key) != true)
                {
                    changeStatus = "NEW\t";
                }
                ConsoleHelper.WriteNotification($"{changeStatus}\t");
            }
            else
            {
                Console.Write("VALID\t");
            }
            
            Console.Write($"{secretDetails.Key}\t");
            
            if (secretDetails.Value.AccessException == null)
            {
                Console.WriteLine($"{secretDetails.Value.DecodedValue}");
            }
            else
            {
                Console.WriteLine($"{secretDetails.Value.AccessException.Status.Detail}");
            }

            compareResult |= hasChanged;
        }
        
        Console.WriteLine();

        return compareResult;
    }

    public static void PrintSecretsMappingIdNamesAccessValues(this IDictionary<string, SecretDetails> secrets)
    {
        var table = new ConsoleTable("secret-id", "environment-variable", "decoded-value");

        foreach (var secretDetails in secrets)
        {
            table.AddRow(secretDetails.Key, 
                secretDetails.Value.EnvironmentVariable, 
                secretDetails.Value.DecodedValue);
        }

        table.Write(Format.Minimal);
    }
}