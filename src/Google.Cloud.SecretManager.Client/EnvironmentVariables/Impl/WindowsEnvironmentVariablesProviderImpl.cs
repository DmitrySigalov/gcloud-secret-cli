using Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Impl;

public class WindowsEnvironmentVariablesProviderImpl : BaseEnvironmentVariablesProvider
{
    public WindowsEnvironmentVariablesProviderImpl(
        IUserFilesProvider userFilesProvider, 
        ILogger<WindowsEnvironmentVariablesProviderImpl> logger) : base(userFilesProvider, logger)
    {
    }

    protected override void OnSetEnvironmentVariable(EnvironmentDescriptor data, Action<string> outputCallback, 
        string name, string value)
    {
        if (value is null)
        {
            OnDeleteEnvironmentVariable(data, outputCallback, name);
            
            return;
        }

#pragma warning disable CA1416 // The code runs only on Windows
        
        using var envKey = Registry.CurrentUser.OpenSubKey("Environment", true);
        
        envKey?.SetValue(name, value);
        
#pragma warning restore CA1416 // The code runs only on Windows
    }

    protected override void OnDeleteEnvironmentVariable(EnvironmentDescriptor data, Action<string> outputCallback, 
        string name)
    {
#pragma warning disable CA1416 // The code runs only on Windows
        
        using var envKey = Registry.CurrentUser.OpenSubKey("Environment", true);
        
        if (envKey?.GetValue(name) != null)
        {
            envKey.DeleteValue(name, throwOnMissingValue: false);
        }
        
#pragma warning restore CA1416 // The code runs only on Windows
    }

    protected override void OnFinishSet(EnvironmentDescriptor data, Action<string> outputCallback)
    {
        // The below code looks dummy but under the hood it sends event to all apps (and awaits for them to handle it)
        // to update Environment Variables in the process memory. Without this line e.g. explorer.exe won't update itself
        // and no app will have new variables until windows is restarted or explorer.exe will restart itself.
        Environment.SetEnvironmentVariable(
            EnvironmentVariablesConsts.GetClientToolVariableName("PROFILE_NAME"), 
            data.ProfileName, 
            EnvironmentVariableTarget.User);
    }
}