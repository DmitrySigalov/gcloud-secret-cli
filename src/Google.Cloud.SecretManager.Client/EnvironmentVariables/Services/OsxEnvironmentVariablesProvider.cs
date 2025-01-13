using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;
using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Services;

public class OsxEnvironmentVariablesProvider : IEnvironmentVariablesProvider
{
    private readonly IUserFilesProvider _userFilesProvider;

    private readonly ILogger<OsxEnvironmentVariablesProvider> _logger;

    private SortedDictionary<string, string> _loadedDescriptor = null;

    public OsxEnvironmentVariablesProvider(
        IUserFilesProvider userFilesProvider,
        ILogger<OsxEnvironmentVariablesProvider> logger)
    {
        _userFilesProvider = userFilesProvider;

        _logger = logger;
    }

    public ISet<string> GetNames(string baseName = null)
    {
        baseName = baseName?.Trim();
        
        var result = LoadEnvironmentVariablesFromDescriptor()
            .Keys
            .ToHashSet();

        if (!string.IsNullOrEmpty(baseName))
        {
            result.ExceptWith(result.Where(x => !x.StartsWith(baseName, StringComparison.InvariantCulture)));
        }

        return result;
    }

    public string Get(string name)
    {
        var environmentVariables = LoadEnvironmentVariablesFromDescriptor();

        if (environmentVariables.TryGetValue(name, out var result))
        {
            return result;
        }

        return null;
    }

    public void Set(string name, string value)
    {
        var environmentVariables = LoadEnvironmentVariablesFromDescriptor();

        if (value == null)
        {
            environmentVariables.Remove(name);
        }
        else
        {
            environmentVariables[name] = value;
        }
        
        DumpEnvironmentVariables(environmentVariables);
        
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
    }

    public string CompleteActivationEnvironmentVariables()
    {
        var updatedScript = _userFilesProvider.GetFullFilePath(
            EnvironmentVariablesConsts.FileNames.ScriptName,
            FolderTypeEnum.ToolUser);

        var resultComment = $"Updated: {updatedScript}";

        var rootFileScriptPath = _userFilesProvider.GetFullFilePath(
            ShellHelper.GetShellScriptFileName(),
            FolderTypeEnum.RootUser);

        if (!File.Exists(rootFileScriptPath))
        {
            resultComment += Environment.NewLine +
                             $"No found script file '{rootFileScriptPath}' according to shell configuration";
        }

        var fileScriptAllText = _userFilesProvider.ReadTextFileIfExist(
            EnvironmentVariablesConsts.FileNames.ScriptExtension,
            FolderTypeEnum.RootUser);

        var partialScriptText = $"source {updatedScript}";
        if (fileScriptAllText?.Contains(partialScriptText) != true)
        {
            fileScriptAllText += Environment.NewLine +
                                 partialScriptText +
                                 Environment.NewLine;

            _userFilesProvider.WriteTextFile(
                EnvironmentVariablesConsts.FileNames.ScriptExtension,
                fileScriptAllText,
                FolderTypeEnum.RootUser);

            resultComment += Environment.NewLine +
                             $"Updated: {rootFileScriptPath}";
        }

        resultComment += Environment.NewLine +
                         $"Reopen application (terminal/rider) or run command in the terminal: source {rootFileScriptPath}";

        return resultComment;
    }

    private SortedDictionary<string, string> LoadEnvironmentVariablesFromDescriptor()
    {
        if (_loadedDescriptor != null)
        {
            return _loadedDescriptor;
        }
        
        var fileDescriptorName = EnvironmentVariablesConsts.FileNames.Descriptor;

        try
        {
            var fileDescriptorText = _userFilesProvider.ReadTextFileIfExist(fileDescriptorName, FolderTypeEnum.ToolUser);

            if (!string.IsNullOrEmpty(fileDescriptorText))
            {
                _loadedDescriptor = JsonSerializationHelper.Deserialize<SortedDictionary<string, string>>(fileDescriptorText);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to read descriptor file with list of active environment variables");
        }

        return _loadedDescriptor ?? new SortedDictionary<string, string>();
    }

    private void DumpEnvironmentVariables(SortedDictionary<string, string> environmentVariables)
    {
        var fileDescriptorName = EnvironmentVariablesConsts.FileNames.Descriptor;
        var fileScriptName = EnvironmentVariablesConsts.FileNames.ScriptName;

        try
        {
            var fileDescriptorText = JsonSerializationHelper.Serialize(environmentVariables);
            _userFilesProvider.WriteTextFile(fileDescriptorName, fileDescriptorText, FolderTypeEnum.ToolUser);

            var fileScriptText = EnvironmentVariablesScriptTextBuilder.Build(environmentVariables);
            _userFilesProvider.WriteTextFile(fileScriptName, fileScriptText, FolderTypeEnum.ToolUser);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to dump descriptor/script file(s) with list of active environment variables");
        }
    }
}