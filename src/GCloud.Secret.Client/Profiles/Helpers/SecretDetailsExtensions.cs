using ConsoleTables;
using Grpc.Core;

namespace GCloud.Secret.Client.Profiles.Helpers;

public static class SecretDetailsExtensions
{
    public static IDictionary<string, string> ToSecretsDictionary(this IDictionary<string, SecretDetails> secrets) =>
        secrets
            .ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value.DecodedValue);
    
    public static SortedDictionary<string, string> ToEnvironmentDictionary(this IDictionary<string, SecretDetails> secrets)
    {
        var result = new SortedDictionary<string, string>();

        foreach (var secret in secrets)
        {
            result[secret.Value.EnvironmentVariable] = secret.Value.DecodedValue;
        }
        
        return result;
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
    
    public static void PrintSecretsMappingIdNamesAccessValues(this IDictionary<string, SecretDetails> secrets)
    {
        var table = new ConsoleTable("secret-id", "config-path", "environment-variable", "decoded-value");

        foreach (var secretDetails in secrets)
        {
            var valueToDisplay = $"<{secretDetails.Value.AccessStatusCode}>";
            if (secretDetails.Value.AccessStatusCode == StatusCode.OK)
            {
                valueToDisplay = secretDetails.Value.DecodedValue;
            }
            
            table.AddRow(secretDetails.Key, 
                secretDetails.Value.ConfigPath, 
                secretDetails.Value.EnvironmentVariable, 
                valueToDisplay);
        }

        table.Write(Format.Minimal);
    }
}