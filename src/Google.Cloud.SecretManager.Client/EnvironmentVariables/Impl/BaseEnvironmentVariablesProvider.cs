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
                    FolderTypeEnum.UserToolConfiguration);

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

    public void Set(EnvironmentDescriptor newData, 
        Action<string> outputCallback)
    {
        var createCounter = 0;
        var updateCounter = 0;
        var deleteCounter = 0;
        
        var currentData = Get() ?? new EnvironmentDescriptor();
        
        try
        {
            if (currentData.ProfileName != newData.ProfileName)
            {
                currentData.ProfileName = newData.ProfileName;
                outputCallback($"Changed active profile to {currentData.ProfileName}");
            }

            // Add/update new variables
            foreach (var newVariable in newData.Variables)
            {
                var availableStatus = currentData.Variables.TryGetValue(newVariable.Key, out var oldValue);
                
                if (!availableStatus || 
                    newVariable.Value != oldValue)
                {
                    OnSetEnvironmentVariable(currentData, outputCallback, newVariable.Key, newVariable.Value);
            
                    currentData.Variables[newVariable.Key] = newVariable.Value;

                    if (availableStatus)
                    {
                        updateCounter++;
                    }
                    else
                    {
                        createCounter++;
                    }
                }
            }
        
            // Delete not actual variables
            var variableNamesToDelete = currentData
                .Variables
                .Where(x => !newData.Variables.ContainsKey(x.Key))
                .Select(x => x.Key)
                .ToArray();
            foreach (var varName in variableNamesToDelete)
            {
                OnDeleteEnvironmentVariable(currentData, outputCallback, varName);
                
                currentData.Variables.Remove(varName);

                deleteCounter++;
            }
        }
        finally
        {
            if (createCounter > 0)
            {
                outputCallback($"Created {createCounter} environment variables");
            }
            if (updateCounter > 0)
            {
                outputCallback($"Updated {updateCounter} environment variables");
            }
            if (deleteCounter > 0)
            {
                outputCallback($"Deleted {deleteCounter} environment variables");
            }
                
            DumpDescriptor(currentData);

            if (createCounter + updateCounter + deleteCounter > 0)
            {
                OnFinishSet(currentData, outputCallback);
            }
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
                FolderTypeEnum.UserToolConfiguration);
        }
        catch (Exception ex)
        {
            Logger?.LogError(
                ex,
                "Error on attempt to save descriptor file with list of active environment variables");
        }
    }
}