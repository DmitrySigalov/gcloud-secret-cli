using ConsoleTables;
using Google.Cloud.SecretManager.Client.Common;

namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public static class ProfileConfigExtensions
{
    public static bool IsValid(this ProfileConfig profileConfig) =>
        !string.IsNullOrEmpty(profileConfig?.ProjectId);

    public static ProfileConfig CloneObject(this ProfileConfig profileConfig)
    {
        var json = JsonSerializationHelper.Serialize(profileConfig);
        
        return JsonSerializationHelper.Deserialize<ProfileConfig>(json);
    }

    public static void PrintProfileSettings(this ProfileConfig profileConfig)
    {
        if (profileConfig == null)
        {
            return;
        }

        var data = JsonSerializationHelper.Serialize(profileConfig);

        Console.WriteLine(data);
        Console.WriteLine();
    }

    public static void PrintProfileSecretIdsNamesMappings(this ProfileConfig profileConfig,
        HashSet<string> secretIds)
    {
        if (profileConfig == null)
        {
            return;
        }
        
        var mapping = SecretDetailsBuilder.Build(secretIds,
            profileConfig);
        
        var table = new ConsoleTable("secret-id", "environment-variable", "config-path");
        
        foreach (var names in mapping)
        {
            table.AddRow(names.Key, 
                names.Value.EnvironmentVariable, 
                names.Value.ConfigPath != names.Key ? names.Value.ConfigPath : "<secret-id>");
        }
        
        table.Write(Format.Minimal);
    }
}