using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;

public static class EnvironmentVariablesRepositoryExtensions
{
    public static IDictionary<string, string> SetFromSecrets(
        this IEnvironmentVariablesProvider environmentVariablesProvider,
        IDictionary<string, string> secrets,
        ProfileConfig profileConfig)
    {
        var result = new SortedDictionary<string, string>();

        foreach (var ssmParam in secrets)
        {
            var envVarName = EnvironmentVariableNameConverter.ConvertFromSecretPath(ssmParam.Key, profileConfig);
            
            environmentVariablesProvider.Set(envVarName, ssmParam.Value);
            
            result[envVarName] = ssmParam.Value;
        }
        
        return result;
    }
    
    public static IDictionary<string, string> GetAll(
        this IEnvironmentVariablesProvider environmentVariablesProvider,
        ProfileConfig profileConfig)
    {
        var result = new SortedDictionary<string, string>();

        if (profileConfig?.IsValid() != true)
        {
            return result;
        }
        
        var convertedEnvironmentVariableBaseNames = profileConfig.PathFilters
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSecretPath(x, profileConfig))
            .ToArray();

        var environmentVariablesToGet = environmentVariablesProvider
            .GetNames(convertedEnvironmentVariableBaseNames);

        if (environmentVariablesToGet.Any() == false)
        {
            return result;
        }

        foreach (var envVarName in environmentVariablesToGet)
        {
            var envVarValue = environmentVariablesProvider.Get(envVarName);
            
            result.Add(envVarName, envVarValue);
        }

        return result;
    }
    
    public static IDictionary<string, string> DeleteAll(
        this IEnvironmentVariablesProvider environmentVariablesProvider,
        ProfileConfig profileConfig)
    {
        var result = new SortedDictionary<string, string>();
        
        var convertedEnvironmentVariableBaseNames = profileConfig.PathFilters
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSecretPath(x, profileConfig))
            .ToArray();

        var environmentVariablesToDelete = environmentVariablesProvider
            .GetNames(convertedEnvironmentVariableBaseNames);

        if (environmentVariablesToDelete.Any() == false)
        {
            return result;
        }

        foreach (var envVarName in environmentVariablesToDelete)
        {
            var envVarValue = environmentVariablesProvider.Get(envVarName);
            
            environmentVariablesProvider.Set(envVarName, null);
            
            result.Add(envVarName, envVarValue);
        }

        return result;
    }
    
    private static ISet<string> GetNames(
        this IEnvironmentVariablesProvider environmentVariablesProvider,
        IEnumerable<string> baseNames)
    {
        return baseNames
            .Select(environmentVariablesProvider.GetNames)
            .SelectMany(x => x)
            .OrderBy(x => x)
            .ToHashSet();
    }
}