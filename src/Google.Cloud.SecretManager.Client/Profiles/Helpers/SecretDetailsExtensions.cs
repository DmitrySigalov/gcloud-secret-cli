using ConsoleTables;
using Grpc.Core;

namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public static class SecretDetailsExtensions
{
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
            table.AddRow(secretDetails.Key, 
                secretDetails.Value.ConfigPath, 
                secretDetails.Value.EnvironmentVariable, 
                secretDetails.Value.AccessStatusCode == StatusCode.OK 
                    ? secretDetails.Value.DecodedValue
                    : $"<{secretDetails.Value.AccessStatusCode}>");
        }

        table.Write(Format.Minimal);
    }
}