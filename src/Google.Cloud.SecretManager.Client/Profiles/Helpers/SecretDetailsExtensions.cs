using ConsoleTables;
using Google.Cloud.SecretManager.Client.Common;

namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public static class SecretDetailsExtensions
{
    public static void PrintSecretsMappingIdNames(this IDictionary<string, SecretDetails> secrets)
    {
        var table = new ConsoleTable("secret-id", "environment-variable", "config-path");

        foreach (var names in secrets)
        {
            table.AddRow(names.Key, 
                names.Value.EnvironmentVariable, 
                names.Value.ConfigPath != names.Key ? names.Value.ConfigPath : "<secret-id>");
        }

        table.Write(Format.Minimal);
    }
    
    public static async Task PrintProgressGetSecretLatestValuesAsync(this IDictionary<string, SecretDetails> secrets,
        Func<KeyValuePair<string, SecretDetails>, Task> secretValueGetter)
    {
        foreach (var secretDetails in secrets)
        {
            await secretValueGetter(secretDetails);
            
            ConsoleHelper.WriteNotification($"{secretDetails.Key}: ");

            if (secretDetails.Value.AccessException == null)
            {
                Console.WriteLine($"{secretDetails.Value.DecodedValue}");
            }
            else
            {
                ConsoleHelper.WriteLineError($"{secretDetails.Value.AccessException.Message}");
            }
        }
        
        Console.WriteLine();
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