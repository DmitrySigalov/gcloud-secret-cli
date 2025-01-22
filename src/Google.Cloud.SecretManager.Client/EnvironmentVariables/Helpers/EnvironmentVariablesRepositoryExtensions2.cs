using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;

public static class EnvironmentVariablesRepositoryExtensions2
{
    public static IDictionary<string, string> SetFromSecrets(
        this IEnvironmentVariablesProvider2 environmentVariablesProvider2,
        IDictionary<string, string> secrets,
        ProfileConfig profileConfig)
    {
        var result = new SortedDictionary<string, string>();

        foreach (var ssmParam in secrets)
        {
            var envVarName = EnvironmentVariableNameConverter.ConvertFromSecretId(ssmParam.Key, profileConfig);
            
            environmentVariablesProvider2.Set(envVarName, ssmParam.Value);
            
            result[envVarName] = ssmParam.Value;
        }
        
        return result;
    }
    
    public static IDictionary<string, string> GetAll(
        this IEnvironmentVariablesProvider2 environmentVariablesProvider2,
        ProfileConfig profileConfig)
    {
        var result = new SortedDictionary<string, string>();

        if (profileConfig?.IsValid() != true)
        {
            return result;
        }
        
        var convertedEnvironmentVariableBaseNames = profileConfig.PathFilters
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSecretId(x, profileConfig))
            .ToArray();

        var environmentVariablesToGet = environmentVariablesProvider2
            .GetNames(convertedEnvironmentVariableBaseNames);

        if (environmentVariablesToGet.Any() == false)
        {
            return result;
        }

        foreach (var envVarName in environmentVariablesToGet)
        {
            var envVarValue = environmentVariablesProvider2.Get(envVarName);
            
            result.Add(envVarName, envVarValue);
        }

        return result;
    }
    
    public static IDictionary<string, string> DeleteAll(
        this IEnvironmentVariablesProvider2 environmentVariablesProvider2,
        ProfileConfig profileConfig)
    {
        var result = new SortedDictionary<string, string>();
        
        var convertedEnvironmentVariableBaseNames = profileConfig.PathFilters
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSecretId(x, profileConfig))
            .ToArray();

        var environmentVariablesToDelete = environmentVariablesProvider2
            .GetNames(convertedEnvironmentVariableBaseNames);

        if (environmentVariablesToDelete.Any() == false)
        {
            return result;
        }

        foreach (var envVarName in environmentVariablesToDelete)
        {
            var envVarValue = environmentVariablesProvider2.Get(envVarName);
            
            environmentVariablesProvider2.Set(envVarName, null);
            
            result.Add(envVarName, envVarValue);
        }

        return result;
    }
    
    private static ISet<string> GetNames(
        this IEnvironmentVariablesProvider2 environmentVariablesProvider2,
        IEnumerable<string> baseNames)
    {
        return baseNames
            .Select(environmentVariablesProvider2.GetNames)
            .SelectMany(x => x)
            .OrderBy(x => x)
            .ToHashSet();
    }
}