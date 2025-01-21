using System.Text;
using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;

namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public static class SecretDetailsBuilder
{
    public static Dictionary<string, SecretDetails> Build(ISet<string> secretIds,
        ProfileConfig profileSettings) =>
        secretIds
            .ToDictionary(
                x => x,
                y => Build(y, profileSettings));

    private static SecretDetails Build(string secretId,
        ProfileConfig profileSettings)
    {
        var result = new SecretDetails();
        
        if (string.IsNullOrEmpty(secretId))
        {
            return result;
        }
        
        result.EnvironmentVariable = ConvertToEnvironmentVariableName(secretId, 
            profileSettings);
        
        result.ConfigPath = ConvertToPath(secretId,
            profileSettings);
        
        return result;
    }
    
    private static string ConvertToEnvironmentVariableName(
        string secretId,
        ProfileConfig profileSettings)
    {
        var result = new StringBuilder();

        result.Append(profileSettings.EnvironmentVariablePrefix);

        if (secretId.StartsWith(profileSettings.SecretIdDelimiter))
        {
            secretId = secretId.TrimStart(profileSettings.SecretIdDelimiter);
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
        string secretId,
        ProfileConfig profileSettings)
    {
        if (profileSettings.ConfigPathDelimiter == profileSettings.SecretIdDelimiter)
        {
            return secretId;
        }
        
        return secretId.Replace(profileSettings.SecretIdDelimiter, 
            profileSettings.ConfigPathDelimiter);
    }
}