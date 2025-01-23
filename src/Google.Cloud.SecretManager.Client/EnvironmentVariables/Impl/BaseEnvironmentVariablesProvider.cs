using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Impl;

public abstract class BaseEnvironmentVariablesProvider : IEnvironmentVariablesProvider
{
    protected BaseEnvironmentVariablesProvider(
        IUserFilesProvider userFilesProvider,
        ILogger logger)
    {
        UserFilesProvider = userFilesProvider;

        Logger = logger;
    }

    protected IUserFilesProvider UserFilesProvider { get; }

    protected ILogger Logger { get; }
    
    public EnvironmentDescriptor Get()
    {
        try
        {
            var fileText = UserFilesProvider
                .ReadTextFileIfExist(EnvironmentVariablesConsts.FileNames.Descriptor, 
                    FolderTypeEnum.ToolUser);

            return JsonSerializationHelper.Deserialize<EnvironmentDescriptor>(fileText);
        }
        catch (Exception ex)
        {
            Logger?.LogError(
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
                OnSetEnvironmentVariable(currentData, outputCallback, newVariable.Key, newVariable.Value);
            
                currentData.Variables[newVariable.Key] = newVariable.Value;
            }
        
            // Delete not actual variables
            var variableNamesToDelete = currentData
                .Variables
                .Where(x => !newData.Variables.ContainsKey(x.Key))
                .Select(x => x.Key);
            foreach (var varName in variableNamesToDelete)
            {
                OnDeleteEnvironmentVariable(currentData, outputCallback, varName);
                
                currentData.Variables.Remove(varName);
            }
        }
        finally
        {
            DumpDescriptor(currentData);

            OnFinishSet(currentData, outputCallback);
        }
    }

    protected abstract void OnSetEnvironmentVariable(EnvironmentDescriptor data, Action<string> outputCallback, 
        string name, string value);

    protected abstract void OnDeleteEnvironmentVariable(EnvironmentDescriptor data, Action<string> outputCallback, 
        string name);

    protected abstract void OnFinishSet(EnvironmentDescriptor data, Action<string> outputCallback);

    private void DumpDescriptor(EnvironmentDescriptor descriptor)
    {
        try
        {
            var fileText = JsonSerializationHelper.Serialize(descriptor);

            UserFilesProvider.WriteTextFile(EnvironmentVariablesConsts.FileNames.Descriptor,
                fileText,
                FolderTypeEnum.ToolUser);
        }
        catch (Exception ex)
        {
            Logger?.LogError(
                ex,
                "Error on attempt to save descriptor file with list of active environment variables");
        }
    }
}