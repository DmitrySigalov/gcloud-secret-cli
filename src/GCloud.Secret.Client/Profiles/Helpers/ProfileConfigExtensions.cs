using System.Text;
using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.EnvironmentVariables.Helpers;

namespace GCloud.Secret.Client.Profiles.Helpers;

public static class ProfileConfigExtensions
{
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
            if (EnvironmentVariablesConsts.InvalidVariableNameCharacters.Contains(c))
            {
                result.Append(EnvironmentVariablesConsts.VariableNameDelimiter);

                continue;
            }

            result.Append(c);
        }

        return result.ToString().Trim().ToUpper();
    }
}