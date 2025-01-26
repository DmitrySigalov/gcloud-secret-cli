namespace GCloud.Secret.Client.EnvironmentVariables.Helpers;

public static class EnvironmentDescriptorExtensions
{
    public static bool HasDiff(this EnvironmentDescriptor newDescriptor,
        IDictionary<string, string> currentEnvironmentVariables)
    {
        currentEnvironmentVariables ??= new Dictionary<string, string>();

        if (newDescriptor.Variables.Count != currentEnvironmentVariables.Count)
        {
            return true;
        }

        foreach (var newVariable in newDescriptor.Variables)
        {
            if (!currentEnvironmentVariables.TryGetValue(newVariable.Key, out var oldValue) ||
                newVariable.Value != oldValue)
            {
                return true;
            }
        }
        
        return currentEnvironmentVariables
            .Any(x => !newDescriptor.Variables.Contains(x));
    }
    
}