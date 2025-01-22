using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Impl;

public abstract class BaseEnvironmentVariablesProvider : IEnvironmentVariablesProvider
{
    private const string FILE_NAME = "environment_descriptor.json";
    
    private readonly ILogger _logger;
    
    protected BaseEnvironmentVariablesProvider(
        IUserFilesProvider userFilesProvider,
        ILogger logger)
    {
        UserFilesProvider = userFilesProvider;

        _logger = logger;
    }

    protected IUserFilesProvider UserFilesProvider { get; }

    public EnvironmentDescriptor Get()
    {
        try
        {
            var fileText = UserFilesProvider
                .ReadTextFileIfExist(FILE_NAME, FolderTypeEnum.ToolUser);

            return JsonSerializationHelper.Deserialize<EnvironmentDescriptor>(fileText);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error on attempt to read descriptor file with list of active environment variables. One of possible reasons - file was corrupted.");

            return null;
        }
    }

    public void Set(EnvironmentDescriptor newData, Action<string> outputCallback)
    {
        var currentData = Get() ?? new EnvironmentDescriptor();
        
        try
        {
            currentData.ProfileName = newData.ProfileName;

            // Add/update new variables
            foreach (var newVariable in newData.Variables)
            {
                OnSetEnvironmentVariable(newVariable.Key, newVariable.Value);
            
                currentData.Variables[newVariable.Key] = newVariable.Value;
            }
        
            // Delete not actual variables
            var variableNamesToDelete = currentData
                .Variables
                .Where(x => !newData.Variables.ContainsKey(x.Key))
                .Select(x => x.Key);
            foreach (var varName in variableNamesToDelete)
            {
                OnDeleteEnvironmentVariable(varName);
            }
        }
        finally
        {
            var fileText = JsonSerializationHelper.Serialize(currentData);
        
            UserFilesProvider.WriteTextFile(FILE_NAME, fileText, FolderTypeEnum.ToolUser);
            
            OnFinishSet(currentData, outputCallback);
        }
    }

    protected abstract void OnSetEnvironmentVariable(string name, string value);

    protected abstract void OnDeleteEnvironmentVariable(string name);

    protected abstract void OnFinishSet(EnvironmentDescriptor data, Action<string> outputCallback);
}