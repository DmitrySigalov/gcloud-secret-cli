using System.Text;
using Google.Cloud.SecretManager.Client.Profiles;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;

public static class EnvironmentVariableNameConverter
{
    public static string ConvertFromSecretId(
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

        return result.ToString().ToUpper();
    }
}