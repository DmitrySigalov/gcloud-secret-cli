namespace Google.Cloud.SecretManager.Client.EnvironmentVariables;

public interface IEnvironmentVariablesProvider2
{
    ISet<string> GetNames(string baseName = null);

    string Get(string name);

    void Set(string name, string value);

    string CompleteActivationEnvironmentVariables();
}