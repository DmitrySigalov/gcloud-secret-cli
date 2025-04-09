using ConsoleTables;
using GCloud.Secret.Client.Common;
using Grpc.Core;

namespace GCloud.Secret.Client.Profiles.Helpers;

public static class SecretDetailsExtensions
{
    private const string NullLabel = "<Null>";
    private const string SeeBelowLabel = "<See below>";

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
        var table = new ConsoleTable("secret-id", "environment-variable");

        foreach (var names in secrets)
        {
            table.AddRow(names.Key, names.Value.EnvironmentVariable);
        }

        table.Write(Format.Minimal);
    }
    
    public static void PrintSecretsMappingIdNamesAccessValues(this IDictionary<string, SecretDetails> secrets)
    {
        var notDisplayedValues = new Dictionary<string, SecretDetails>();
        
        var table = new ConsoleTable("secret-id", "environment-variable", "decoded-value");

        if (secrets.Any())
        {
            var maxAllowedDecodedValueSLengthToDisplay = Console.BufferWidth 
                                                         - (table.Columns.Count + 1) * 3 // Number of column-splitters
                                                         - secrets.Keys.Max(x => x?.Length ?? 0)
                                                         - secrets.Values.Max(x => x?.EnvironmentVariable?.Length ?? 0);
            
            foreach (var secretDetails in secrets)
            {
                var valueToDisplay = secretDetails.Value.DecodedValue;

                if (secretDetails.Value.AccessStatusCode != StatusCode.OK)
                {
                    valueToDisplay = $"<{secretDetails.Value.AccessStatusCode}>";
                }
                else if (valueToDisplay == null)
                {
                    valueToDisplay = NullLabel;
                }
                else if (valueToDisplay.Length > maxAllowedDecodedValueSLengthToDisplay)
                {
                    valueToDisplay = SeeBelowLabel;
                    notDisplayedValues[secretDetails.Key] = secretDetails.Value;
                }
                else if (valueToDisplay.Contains('\n') || valueToDisplay.Contains(Environment.NewLine))
                {
                    valueToDisplay = SeeBelowLabel;
                    notDisplayedValues[secretDetails.Key] = secretDetails.Value;
                }

                table.AddRow(
                    secretDetails.Key,
                    secretDetails.Value.EnvironmentVariable,
                    valueToDisplay);
            }
        }

        table.Write(Format.Minimal);

        if (notDisplayedValues.Any())
        {
            foreach (var secretDetails in notDisplayedValues)
            {
                ConsoleHelper.WriteLineWarn(secretDetails.Key + ": ");
                Console.WriteLine(secretDetails.Value.DecodedValue);
                Console.WriteLine();
            }
        }
    }
}