using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.EnvironmentVariables;
using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;
using Google.Cloud.SecretManager.Client.Profiles.Helpers;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.Profiles.Impl;

public class ProfileConfigProvider : IProfileConfigProvider
{
    private readonly IUserFilesProvider _userFilesProvider;

    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

    private readonly ILogger<ProfileConfigProvider> _logger;

    public ProfileConfigProvider(
        IUserFilesProvider userFilesProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ILogger<ProfileConfigProvider> logger)
    {
        _userFilesProvider = userFilesProvider;

        _environmentVariablesProvider = environmentVariablesProvider;

        _logger = logger;
    }
    
    public string ActiveName
    {
        get => _environmentVariablesProvider.Get(EnvironmentVariablesConsts.GetClientToolVariableName(nameof(ActiveName)));
        set => _environmentVariablesProvider.Set(EnvironmentVariablesConsts.GetClientToolVariableName(nameof(ActiveName)), value);
    }
    
    public ISet<string> GetNames()
    {
        return _userFilesProvider
            .GetFileNames(ProfileFileNameResolver.SearchFileNamePattern, FolderTypeEnum.ToolUser)
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
                .ReadTextFileIfExist(fileName, FolderTypeEnum.ToolUser);

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
        var fileName = ProfileFileNameResolver.BuildFileName(name);

        try
        {
            var fileText = JsonSerializationHelper.Serialize(data);
        
            _userFilesProvider.WriteTextFile(fileName, fileText, FolderTypeEnum.ToolUser);
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
        var fileName = ProfileFileNameResolver.BuildFileName(name);

        try
        {
            _userFilesProvider.DeleteFile(fileName, FolderTypeEnum.ToolUser);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to delete profile file");
        }
    }
}