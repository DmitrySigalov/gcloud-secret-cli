using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Impl;

public class OsxEnvironmentVariablesProviderImpl : BaseEnvironmentVariablesProvider
{
    public OsxEnvironmentVariablesProviderImpl(
        IUserFilesProvider userFilesProvider, 
        ILogger<OsxEnvironmentVariablesProviderImpl> logger) : base(userFilesProvider, logger)
    {
    }

    protected override void OnSetEnvironmentVariable(EnvironmentDescriptor data, Action<string> outputCallback, 
        string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
    }

    protected override void OnDeleteEnvironmentVariable(EnvironmentDescriptor data, Action<string> outputCallback, 
        string name)
    {
        Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.Process);
    }

    protected override void OnFinishSet(EnvironmentDescriptor data, Action<string> outputCallback)
    {
        DumpScriptFile(data, outputCallback);
        
        ConfigureShellScript(data, outputCallback);
    }

    private void DumpScriptFile(EnvironmentDescriptor descriptor, Action<string> outputCallback)
    {
        var fileScriptName = EnvironmentVariablesConsts.FileNames.ScriptName;
        
        try
        {
            var fileScriptText = EnvironmentVariablesScriptTextBuilder.Build(descriptor.Variables);

            UserFilesProvider.WriteTextFile(fileScriptName, 
                fileScriptText, 
                FolderTypeEnum.ToolUser);
            
            outputCallback($"Dumped [{fileScriptName}] file with activated environment variables");
        }
        catch (Exception e)
        {
            Logger.LogError(
                e,
                $"Error on attempt to dump [{fileScriptName}] file with list of activated environment variables");
        }
    }
    
    private void ConfigureShellScript(EnvironmentDescriptor data, Action<string> outputCallback)
    {
        try
        {
            var scriptFilePath = UserFilesProvider.GetFullFilePath(
                EnvironmentVariablesConsts.FileNames.ScriptName,
                FolderTypeEnum.ToolUser);

            var rootFileScriptPath = UserFilesProvider.GetFullFilePath(
                ShellHelper.GetShellScriptFileName(),
                FolderTypeEnum.RootUser);

            if (!File.Exists(rootFileScriptPath))
            {
                outputCallback($"No found script file '{rootFileScriptPath}' according to shell configuration");
            }

            var fileScriptAllText = UserFilesProvider.ReadTextFileIfExist(
                EnvironmentVariablesConsts.FileNames.ScriptExtension,
                FolderTypeEnum.RootUser);

            var partialScriptText = $"source {scriptFilePath}";
            if (fileScriptAllText?.Contains(partialScriptText) != true)
            {
                fileScriptAllText += Environment.NewLine +
                                     partialScriptText +
                                     Environment.NewLine;

                UserFilesProvider.WriteTextFile(
                    EnvironmentVariablesConsts.FileNames.ScriptExtension,
                    fileScriptAllText,
                    FolderTypeEnum.RootUser);

                outputCallback($"Added/configured [{partialScriptText}] in the {EnvironmentVariablesConsts.FileNames.ScriptExtension} file");
            }

            outputCallback($"Reopen application (terminal/rider) or run command in the terminal: source {rootFileScriptPath}");
        }
        catch (Exception e)
        {
            Logger.LogError(
                e,
                "Error on attempt to configure/connect shell to script file with environment variables");
        }
    }
}