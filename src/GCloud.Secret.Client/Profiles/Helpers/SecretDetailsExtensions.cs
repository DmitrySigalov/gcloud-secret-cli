using ConsoleTables;
using GCloud.Secret.Client.Common;

namespace GCloud.Secret.Client.Profiles.Helpers;

public static class SecretDetailsExtensions
{
    private const string SeeBelow = "<See below>";

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
        
        var table = new ConsoleTable("secret-id", "environment-variable", "access-status", "decoded-value");

        if (secrets.Any())
        {
            var maxAllowedDecodedValueSLengthToDisplay = Console.BufferWidth 
                                                         - (table.Columns.Count + 1) * 2 // Number of splitted columns
                                                         - secrets.Keys.Max(x => x?.Length ?? 0)
                                                         - secrets.Values.Max(x => x?.DecodedValue?.Length ?? 0)
                                                         - secrets.Values.Max(x => x?.AccessStatusCode.ToString().Length ?? 0);
            
            foreach (var secretDetails in secrets)
            {
                var status = secretDetails.Value.AccessStatusCode.ToString();
                var valueToDisplay = secretDetails.Value.DecodedValue;

                if (valueToDisplay != null)
                {
                    if (valueToDisplay.Length > maxAllowedDecodedValueSLengthToDisplay)
                    {
                        valueToDisplay = SeeBelow;
                        notDisplayedValues[secretDetails.Key] = secretDetails.Value;
                    }
                    else if (valueToDisplay.Contains('\n') || valueToDisplay.Contains(Environment.NewLine))
                    {
                        valueToDisplay = SeeBelow;
                        notDisplayedValues[secretDetails.Key] = secretDetails.Value;
                    }
                }

                table.AddRow(
                    secretDetails.Key,
                    secretDetails.Value.EnvironmentVariable,
                    status,
                    valueToDisplay);
            }
        }

        table.Write(Format.Minimal);

        if (notDisplayedValues.Any())
        {
            foreach (var secretDetails in notDisplayedValues)
            {
                ConsoleHelper.WriteWarn(secretDetails.Key + ": ");
                Console.WriteLine(secretDetails.Value.DecodedValue);
            }
            Console.WriteLine();
        }
    }
}