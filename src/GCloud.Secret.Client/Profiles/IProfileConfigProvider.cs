namespace Google.Cloud.SecretManager.Client.Profiles;

public interface IProfileConfigProvider
{
    ISet<string> GetNames();

    ProfileConfig GetByName(string name);

    void Save(string name, ProfileConfig data);

    void Delete(string name);

    IDictionary<string, SecretDetails> ReadSecrets(string name);

    void DumpSecrets(string name, IDictionary<string, SecretDetails> data);
}