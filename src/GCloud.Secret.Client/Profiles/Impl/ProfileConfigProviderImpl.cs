using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.Profiles.Impl;

public class ProfileConfigProviderImpl : IProfileConfigProvider
{
    private readonly IUserFilesProvider _userFilesProvider;

    private readonly ILogger<ProfileConfigProviderImpl> _logger;

    public ProfileConfigProviderImpl(
        IUserFilesProvider userFilesProvider,
        ILogger<ProfileConfigProviderImpl> logger)
    {
        _userFilesProvider = userFilesProvider;

        _logger = logger;
    }
    
    public ISet<string> GetNames()
    {
        return _userFilesProvider
            .GetFileNames(ProfileFileNameResolver.SearchFileNamePattern, FolderTypeEnum.UserToolConfiguration)
            .Select(ProfileFileNameResolver.ExtractProfileName)
            .OrderBy(x => x)
            .ToHashSet();
    }

    public ProfileConfig GetByName(string name)
    {
        var fileName = ProfileFileNameResolver.BuildFileName(name);

        try
        {
            var fileText = _userFilesProvider
                .ReadTextFileIfExist(fileName, FolderTypeEnum.UserToolConfiguration);

            return JsonSerializationHelper.Deserialize<ProfileConfig>(fileText);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to read profile configuration. One of possible reasons - file was corrupted. Continue with default settings.");

            return null;
        }
    }

    public void Save(string name, ProfileConfig data)
    {
        var profileFileName = ProfileFileNameResolver.BuildFileName(name);

        try
        {
            var fileText = JsonSerializationHelper.Serialize(data);
        
            _userFilesProvider.WriteTextFile(profileFileName, fileText, FolderTypeEnum.UserToolConfiguration);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to save profile configuration. Continue with default settings.");
        }
    }

    public void Delete(string name)
    {
        var secretsFileName = SecretsFileNameResolver.BuildFileName(name);
        var profileFileName = ProfileFileNameResolver.BuildFileName(name);

        try
        {
            _userFilesProvider.DeleteFile(secretsFileName, FolderTypeEnum.UserToolConfiguration);
            _userFilesProvider.DeleteFile(profileFileName, FolderTypeEnum.UserToolConfiguration);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to delete profile file");
        }
    }

    public IDictionary<string, SecretDetails> ReadSecrets(string name)
    {
        var fileName = SecretsFileNameResolver.BuildFileName(name);

        try
        {
            var fileText = _userFilesProvider
                .ReadTextFileIfExist(fileName, FolderTypeEnum.UserToolConfiguration);

            return JsonSerializationHelper.Deserialize<Dictionary<string, SecretDetails>>(fileText);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void DumpSecrets(string name, IDictionary<string, SecretDetails> data)
    {
        var fileName = SecretsFileNameResolver.BuildFileName(name);

        try
        {
            var fileText = JsonSerializationHelper.Serialize(data);
        
            _userFilesProvider.WriteTextFile(fileName, fileText, FolderTypeEnum.UserToolConfiguration);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to dump secrets");
        }
    }
}