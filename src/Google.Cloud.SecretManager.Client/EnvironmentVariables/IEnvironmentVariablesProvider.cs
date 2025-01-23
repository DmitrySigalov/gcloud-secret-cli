namespace Google.Cloud.SecretManager.Client.EnvironmentVariables;

public interface IEnvironmentVariablesProvider
{
    EnvironmentDescriptor Get();

    void Set(EnvironmentDescriptor newData,
        bool skipCheckChanges,
        Action<string> outputCallback);
}