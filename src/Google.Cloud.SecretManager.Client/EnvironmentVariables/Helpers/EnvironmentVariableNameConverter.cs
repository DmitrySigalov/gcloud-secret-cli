using System.Text;
using Google.Cloud.SecretManager.Client.Profiles;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;

public static class EnvironmentVariableNameConverter
{
    public static string ConvertFromSecretPath(
        string secretPath,
        ProfileConfig profileSettings)
    {
        var result = new StringBuilder();

        // result.Append(profileSettings.EnvironmentVariablePrefix);
        //
        // if (secretPath.StartsWith(profileSettings.BaseDelimiter))
        // {
        //     secretPath = secretPath.TrimStart(profileSettings.BaseDelimiter);
        // }

        foreach (var c in secretPath)
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