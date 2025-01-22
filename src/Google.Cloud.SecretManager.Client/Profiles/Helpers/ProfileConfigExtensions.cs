using System.Text;
using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;

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

    public static void PrintProfileConfig(this ProfileConfig profileConfig)
    {
        if (profileConfig == null)
        {
            return;
        }

        var data = JsonSerializationHelper.Serialize(profileConfig);

        Console.WriteLine(data);
        Console.WriteLine();
    }
    
    public static Dictionary<string, SecretDetails> BuildSecretDetails(this ProfileConfig profileConfig,
        ISet<string> secretIds) =>
        secretIds
            .ToDictionary(
                x => x,
                profileConfig.BuildSecretDetails);

    private static SecretDetails BuildSecretDetails(this ProfileConfig profileConfig,
        string secretId)
    {
        var result = new SecretDetails();
        
        if (string.IsNullOrEmpty(secretId))
        {
            return result;
        }
        
        result.ConfigPath = profileConfig.ConvertToPath(secretId);
        
        result.EnvironmentVariable = profileConfig.ConvertToEnvironmentVariableName(secretId);
        
        return result;
    }
    
    private static string ConvertToEnvironmentVariableName(
        this ProfileConfig profileConfig,
        string secretId)
    {
        var result = new StringBuilder();

        result.Append(profileConfig.EnvironmentVariablePrefix);

        if (secretId.StartsWith(profileConfig.SecretIdDelimiter) &&
            profileConfig.RemoveStartDelimiter)
        {
            secretId = secretId.TrimStart(profileConfig.SecretIdDelimiter);
        }
        
        foreach (var c in secretId)
        {
            if (EnvironmentVariableNameValidationRule.InvalidVariableNameCharacters.Contains(c))
            {
                result.Append(EnvironmentVariablesConsts.VariableNameDelimeter);

                continue;
            }

            result.Append(c);
        }

        return result.ToString().Trim().ToUpper();
    }
    
    private static string ConvertToPath(
        this ProfileConfig profileConfig,
        string secretId)
    {
        if (profileConfig.SecretPathDelimiter == profileConfig.SecretIdDelimiter)
        {
            return secretId;
        }
        
        return secretId.Replace(profileConfig.SecretIdDelimiter, 
            profileConfig.SecretPathDelimiter);
    }
}